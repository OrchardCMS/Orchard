using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.Environment.Extensions.Loaders {
    public class CSharpProjectDescriptor {
        public IEnumerable<string> SourceFilenames { get; set; }
        public IEnumerable<ReferenceDescriptor> References { get; set; }
    }

    public class ReferenceDescriptor {
        public string AssemblyName { get; set; }
    }

    public class CSharpProjectParser {
        public CSharpProjectDescriptor Parse(Stream stream) {
            var document = XDocument.Load(XmlReader.Create(stream));
            return new CSharpProjectDescriptor {
                SourceFilenames = GetSourceFilenames(document),
                References = GetReferences(document)
            };
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