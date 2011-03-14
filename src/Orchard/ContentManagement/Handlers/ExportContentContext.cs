using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ExportContentContext : ContentContextBase {
        public XElement Data { get; set; }

        public ExportContentContext(ContentItem contentItem, XElement data)
            : base(contentItem) {
            Data = data;
        }
    }
}