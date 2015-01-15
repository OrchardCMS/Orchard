using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ImportContentContext : ContentContextBase {
        public XElement Data { get; set; }
        public IList<FileToImport> Files { get; set; }
        public ImportContentSession Session { get; set; }

        protected ImportContentContext(ContentItem contentItem) : base(contentItem) {
            Files = new List<FileToImport>();
        }

        public ImportContentContext(ContentItem contentItem, XElement data, IEnumerable<FileToImport> files, ImportContentSession importContentSession)
            : base(contentItem) {
            Files = files == null ? new List<FileToImport>() : files.ToList();
            Data = data;
            Session = importContentSession;
        }

        public ImportContentContext(XElement data, IEnumerable<FileToImport> files, ImportContentSession importContentSession)
            : this(null, data, files, importContentSession) { }

        public string Attribute(string elementName, string attributeName) {
            var element = Data.Element(elementName);
            if (element != null) {
                var attribute = element.Attribute(attributeName);
                if (attribute != null)
                    return attribute.Value;
            }
            return null;
        }

        public void ImportAttribute(string elementName, string attributeName, Action<string> value) {
            ImportAttribute(elementName, attributeName, value, () => { });
        }

        public void ImportAttribute(string elementName, string attributeName, Action<string> value, Action empty) {
            var importedText = Attribute(elementName, attributeName);
            if (importedText != null) {
                try {
                    value(importedText);
                }
                catch {
                    empty();
                }
            }
            else {
                empty();
            }
        }

        public ContentItem GetItemFromSession(string id) {
            return GetItemFromSession(id, VersionOptions.Latest);
        }

        public ContentItem GetItemFromSession(string id, VersionOptions versionOptions, string contentTypeHint = null) {
            return Session.Get(id, versionOptions, contentTypeHint);
        }
    }

    public class FileToImport {
        public string Path { get; set; }
        public Func<Stream> GetStream { get; set; }
    }
}