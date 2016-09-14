using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Xml;

namespace XmlDocStripTask
{
    public class StripTask : Task
    {
        [Required]
        public string XmlDocumentationFilename { get; set; }

        [Required]
        public string AssemblyFilename { get; set; }

        public override bool Execute()
        {
            base.Log.LogMessage(MessageImportance.High, "Optimizing XML documentation...");
            if (!File.Exists(XmlDocumentationFilename))
            {
                base.Log.LogWarning("XML documentation file not found: " + XmlDocumentationFilename);
                return true;
            }
            if (!File.Exists(AssemblyFilename))
            {
                base.Log.LogWarning("Assembly not found: " + AssemblyFilename);
                return true;
            }

            FileInfo fi = new FileInfo(XmlDocumentationFilename);
            string outFilename = fi.FullName.Replace(fi.Extension, ".intellisense" + fi.Extension);
            try
            {
                Strip(XmlDocumentationFilename, AssemblyFilename, outFilename);
                base.Log.LogMessage(MessageImportance.High, "XML documentation optimization complete.");
            }
            catch(System.Exception ex)
            {
                base.Log.LogWarning("XML documentation stripping failed with error: " + ex.Message);
            }
            return true;
        }

        private static void Strip(string xmlDoc, string assemblyName, string outFilename)
        {
            var xmldoc = new System.Xml.XmlDocument();
            xmldoc.Load(xmlDoc);

            using (var module = Mono.Cecil.ModuleDefinition.ReadModule(assemblyName))
            {
                //Build map of all public members
                var publicTypes = new Dictionary<string, MemberReference>();
                foreach (var item in module.GetTypes().Where(t => IsPublicApi(t)))
                {
                    publicTypes.Add(DocCommentId.GetDocCommentId(item), item);
                    foreach (var m in item.Methods.Where(m => IsPublicApi(m)))
                    {   
                        publicTypes.Add(DocCommentId.GetDocCommentId(m), m);
                    }
                    foreach (var p in item.Properties.Where( m => 
                        m.GetMethod != null && IsPublicApi(m.GetMethod) || 
                        m.SetMethod != null && IsPublicApi(m.SetMethod)))
                    {
                        publicTypes.Add(DocCommentId.GetDocCommentId(p), p);
                    }
                    foreach (var e in item.Events.Where(m => m.AddMethod != null && IsPublicApi(m.AddMethod) || m.RemoveMethod != null && IsPublicApi(m.RemoveMethod)))
                    {
                        publicTypes.Add(DocCommentId.GetDocCommentId(e), e);
                    }
                    foreach (var f in item.Fields.Where(m => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly))
                    {
                        publicTypes.Add(DocCommentId.GetDocCommentId(f), f);
                    }
                }

                var membersNode = xmldoc.DocumentElement.SelectSingleNode("members");
                var members = membersNode.SelectNodes("member");
                foreach (var member in members.OfType<System.Xml.XmlNode>().ToArray())
                {
                    var docCommentId = member.Attributes["name"].Value;
                    if (!publicTypes.ContainsKey(docCommentId))
                    {
                        //Not public. Remove
                        membersNode.RemoveChild(member);
                        continue;
                    }
                    //Strip out everything but summary, param, returns, exception, value
                    foreach (var child in member.ChildNodes.OfType<XmlNode>().ToArray())
                    {
                        string name = child.Name;
                        if (name == "summary" || name == "param" || name == "returns" || name == "exception" || name == "value")
                        {
                            if (child.InnerText != null)
                            {
                                var lines = child.InnerXml.Split(new char[] { '\n' });
                                child.InnerXml = string.Join(" ", lines.Select(l => l.Trim())).Trim();
                            }
                            continue;
                        }
                        member.RemoveChild(child);
                    }
                }
            }
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                NewLineChars = string.Empty,
                Indent = false,
                CheckCharacters = true
            };
            using (var writer = XmlWriter.Create(outFilename, settings))
            {
                xmldoc.Save(writer);
            }
        }

        private static bool IsPublicApi(TypeDefinition type)
        {
            return type.IsNested ?
                type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamilyOrAssembly :
                type.IsPublic;
        }

        private static bool IsPublicApi(MethodDefinition m)
        {
            return m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
        }
    }
}
