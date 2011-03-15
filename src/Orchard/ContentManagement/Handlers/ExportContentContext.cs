using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ExportContentContext : ContentContextBase {
        public XElement Data { get; set; }

        public ExportContentContext(ContentItem contentItem, XElement data)
            : base(contentItem) {
            Data = data;
        }

        public XElement Element(string elementName) {
            var element = Data.Element(elementName);
            if (element == null) {
                element = new XElement(elementName);
                Data.Add(element);
            }
            return element;
        }
    }
}