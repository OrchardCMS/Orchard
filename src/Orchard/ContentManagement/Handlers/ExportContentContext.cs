using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ExportContentContext : ContentContextBase {
        public string Prefix { get; set; }
        public XElement Data { get; set; }

        /// <summary>
        /// Wether the content item should be exclude from the export or not
        /// </summary>
        public bool Exclude { get; set; }

        private readonly string Separator = @".";

        public ExportContentContext(ContentItem contentItem, XElement data)
            : base(contentItem) {
            Data = data;
        }

        public XElement Element(string elementName) {
            if (!string.IsNullOrEmpty(Prefix))
                elementName = string.Join(Separator, Prefix, elementName);

            var element = Data.Element(elementName);
            if (element == null) {
                element = new XElement(elementName);
                Data.Add(element);
            }
            return element;
        }
    }
}