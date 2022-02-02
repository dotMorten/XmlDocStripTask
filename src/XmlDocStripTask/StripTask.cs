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
        public string AssemblyFilename { get; set; }

        public string OutputPath { get; set; }

        public bool PreserveRemarks { get; set; } = false;

        public override bool Execute()
        {
            var assemblyFileInfo = new FileInfo(AssemblyFilename);
            string XmlDocumentationFilename = assemblyFileInfo.FullName.Substring(0, assemblyFileInfo.FullName.Length - assemblyFileInfo.Extension.Length) + ".xml";
            base.Log.LogMessage(MessageImportance.High, $"Optimizing XML Documentation '{XmlDocumentationFilename}' for assembly '{AssemblyFilename}'");
            if (!File.Exists(XmlDocumentationFilename))
            {
                base.Log.LogWarning("XMLStrip", "XMLStrip0001", null, null, 0, 0, 0, 0, "XML documentation file not found: " + XmlDocumentationFilename);
                return true;
            }
            if (!File.Exists(AssemblyFilename))
            {
                base.Log.LogWarning("XMLStrip", "XMLStrip0002", null, null, 0, 0, 0, 0, "Assembly not found: " + AssemblyFilename);
                return true;
            }
            FileInfo fi = new FileInfo(XmlDocumentationFilename);
            string outFilename = OutputPath;
            if (string.IsNullOrEmpty(outFilename))
                outFilename = XmlDocumentationFilename;
            try
            {
                var oldFileLength = new FileInfo(XmlDocumentationFilename).Length;
                Strip(XmlDocumentationFilename, AssemblyFilename, outFilename);
                var newFileLength = new FileInfo(outFilename).Length;
                base.Log.LogMessage(MessageImportance.High, $"Reduced XML Documentation by {((1 - (newFileLength / (double)oldFileLength)) * 100).ToString("0.0")}% ({oldFileLength} bytes => {newFileLength} bytes)");
            }
            catch(System.Exception ex)
            {
                base.Log.LogWarning("XMLStrip", "XMLStrip0003", null, null, 0, 0, 0, 0, "XML documentation stripping failed with error: " + ex.Message);
            }
            return true;
        }

        private void Strip(string xmlDoc, string assemblyName, string outFilename)
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
                foreach (var comment in membersNode.OfType<System.Xml.XmlComment>().ToArray())
                {
                    membersNode.RemoveChild(comment);
                }
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
                        if (name == "summary" || name == "param" || name == "returns" || name == "exception" || name == "value" || (PreserveRemarks && name == "remarks"))
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
                writer.Flush();
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
