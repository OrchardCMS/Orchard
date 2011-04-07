using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ImportContentContext : ContentContextBase {
        public XElement Data { get; set; }
        private ImportContentSession Session { get; set; }

        public ImportContentContext(ContentItem contentItem, XElement data, ImportContentSession importContentSession)
            : base(contentItem) {
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

        public ContentItem GetItemFromSession(string id) {
            return Session.Get(id);
        }

    }
}