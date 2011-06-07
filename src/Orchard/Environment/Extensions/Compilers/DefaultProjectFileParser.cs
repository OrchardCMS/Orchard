using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Environment.Extensions.Compilers {
    public class DefaultProjectFileParser : IProjectFileParser {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly ICacheManager _cacheManager;

        public DefaultProjectFileParser(IWebSiteFolder webSiteFolder, ICacheManager cacheManager) {
            _webSiteFolder = webSiteFolder;
            _cacheManager = cacheManager;
        }

        public ProjectFileDescriptor Parse(string virtualPath) {
            return _cacheManager.Get(virtualPath,
                ctx => {
                    ctx.Monitor(_webSiteFolder.WhenPathChanges(virtualPath));
                    string content = _webSiteFolder.ReadFile(virtualPath);
                    using (var reader = new StringReader(content)) {
                        return Parse(reader);
                    }
                });
        }

        public ProjectFileDescriptor Parse(Stream stream) {
            using (var reader = new StreamReader(stream)) {
                return Parse(reader);
            }
        }

        public ProjectFileDescriptor Parse(TextReader reader) {
            var document = XDocument.Load(XmlReader.Create(reader));
            return new ProjectFileDescriptor {
                AssemblyName = GetAssemblyName(document),
                SourceFilenames = GetSourceFilenames(document).ToArray(),
                References = GetReferences(document).ToArray()
            };
        }

        private static string GetAssemblyName(XDocument document) {
            return document
                .Elements(ns("Project"))
                .Elements(ns("PropertyGroup"))
                .Elements(ns("AssemblyName"))
                .Single()
                .Value;
        }

        private static IEnumerable<string> GetSourceFilenames(XDocument document) {
            return document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("Compile"))
                .Attributes("Include")
                .Select(c => c.Value);
        }

        private static IEnumerable<ReferenceDescriptor> GetReferences(XDocument document) {
            var assemblyReferences = document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("Reference"))
                .Where(c => c.Attribute("Include") != null)
                .Select(c => {
                            string path = null;
                            XElement attribute = c.Elements(ns("HintPath")).FirstOrDefault();
                            if (attribute != null) {
                                path = attribute.Value;
                            }

                            return new ReferenceDescriptor {
                                SimpleName = ExtractAssemblyName(c.Attribute("Include").Value),
                                FullName = c.Attribute("Include").Value,
                                Path = path,
                                ReferenceType = ReferenceType.Library
                            };
                        });

            var projectReferences = document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("ProjectReference"))
                .Attributes("Include")
                .Select(c => new ReferenceDescriptor {
                    SimpleName = Path.GetFileNameWithoutExtension(c.Value),
                    FullName = Path.GetFileNameWithoutExtension(c.Value),
                    Path = c.Value,
                    ReferenceType = ReferenceType.Project
                });

            return assemblyReferences.Union(projectReferences);
        }

        private static string ExtractAssemblyName(string value) {
            int index = value.IndexOf(',');
            return index < 0 ? value : value.Substring(0, index);
        }

        private static XName ns(string name) {
            return XName.Get(name, "http://schemas.microsoft.com/developer/msbuild/2003");
        }
    }
}