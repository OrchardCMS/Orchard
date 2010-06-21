using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.Environment.Extensions.Compilers {
    public class DefaultProjectFileParser : IProjectFileParser {

        public ProjectFileDescriptor Parse(Stream stream) {
            var document = XDocument.Load(XmlReader.Create(stream));
            return new ProjectFileDescriptor {
                AssemblyName = GetAssemblyName(document),
                SourceFilenames = GetSourceFilenames(document).ToArray(),
                References = GetReferences(document).ToArray()
            };
        }

        private string GetAssemblyName(XDocument document) {
            return document
                .Elements(ns("Project"))
                .Elements(ns("PropertyGroup"))
                .Elements(ns("AssemblyName"))
                .Single()
                .Value;
        }

        private IEnumerable<string> GetSourceFilenames(XDocument document) {
            return document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("Compile"))
                .Attributes("Include")
                .Select(c => c.Value);
        }

        private IEnumerable<ReferenceDescriptor> GetReferences(XDocument document) {
            return document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("Reference"))
                .Attributes("Include")
                .Select(c => new ReferenceDescriptor { AssemblyName = c.Value });
        }

        private static XName ns(string name) {
            return XName.Get(name, "http://schemas.microsoft.com/developer/msbuild/2003");
        }
    }
}