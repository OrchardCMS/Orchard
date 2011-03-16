using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ImportContentContext : ContentContextBase {
        public XElement Data { get; set; }

        public ImportContentContext(ContentItem contentItem, XElement data)
            : base(contentItem) {
            Data = data;
        }

        public XElement Element(string elementName) {
            return Data.Element(elementName);
        }
    }
}