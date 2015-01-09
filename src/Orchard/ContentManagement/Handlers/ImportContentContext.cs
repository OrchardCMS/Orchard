using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ImportContentContext : ContentContextBase {
        public XElement Data { get; set; }
        public IList<ExportedFileDescription> Files { get; set; }
        private ImportContentSession Session { get; set; }

        protected ImportContentContext(ContentItem contentItem) : base(contentItem) {
            Files = new List<ExportedFileDescription>();
        }

        public ImportContentContext(ContentItem contentItem, XElement data, ImportContentSession importContentSession)
            : this(contentItem) {
            Data = data;
            Session = importContentSession;
        }

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
}