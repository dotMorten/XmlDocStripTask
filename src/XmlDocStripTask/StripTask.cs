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
            if (!File.Exists(XmlDocumentationFilename))
                return true;
            if (!File.Exists(AssemblyFilename))
                return true;

            FileInfo fi = new FileInfo(XmlDocumentationFilename);
            string outFilename = fi.FullName.Replace(fi.Extension, ".intellisense" + fi.Extension);
            try
            {
                Strip(XmlDocumentationFilename, AssemblyFilename, outFilename);
            }
            catch(System.Exception ex)
            {
                base.Log.LogWarning("XML Documentation stripping failed with error: " + ex.Message);
            }
            return true;
        }


        private static void Strip(string xmlDoc, string assemblyName, string outFilename)
        {
            //  string xmlDoc = args[1];
            //  string assemblyName = args[2];
            //  string outFilename = args[3];

            var xmldoc = new System.Xml.XmlDocument();
            xmldoc.Load(xmlDoc);

            using (var module = Mono.Cecil.ModuleDefinition.ReadModule(assemblyName))
            {
                //Build map of all public members
                var publicTypes = new Dictionary<string, MemberReference>();
                foreach (var item in module.GetTypes().Where(t => t.IsPublic))
                {
                    publicTypes.Add(DocCommentId.GetDocCommentId(item), item);
                    foreach (var m in item.Methods.Where(m => m.IsPublic))
                    {
                        publicTypes.Add(DocCommentId.GetDocCommentId(m), m);
                    }
                    foreach (var p in item.Properties.Where(m => m.GetMethod != null && m.GetMethod.IsPublic || m.SetMethod != null && m.SetMethod.IsPublic))
                    {
                        publicTypes.Add(DocCommentId.GetDocCommentId(p), p);
                    }
                    foreach (var e in item.Events.Where(m => m.AddMethod != null && m.AddMethod.IsPublic || m.RemoveMethod != null && m.RemoveMethod.IsPublic))
                    {
                        publicTypes.Add(DocCommentId.GetDocCommentId(e), e);
                    }
                    foreach (var f in item.Fields.Where(m => m.IsPublic))
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
    }
}
