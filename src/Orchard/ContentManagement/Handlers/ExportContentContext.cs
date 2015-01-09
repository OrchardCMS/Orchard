using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ExportContentContext : ContentContextBase {
        public XElement Data { get; set; }
        protected IList<ExportedFileDescription> Files { get; set; }

        /// <summary>
        /// Wether the content item should be exclude from the export or not
        /// </summary>
        public bool Exclude { get; set; }

        protected ExportContentContext(ContentItem contentItem) : base(contentItem) {
            Files = new List<ExportedFileDescription>();
        }

        public ExportContentContext(ContentItem contentItem, XElement data)
            : this(contentItem) {
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

        public void AddFile(string localPath, Stream contents) {
            if (Files.Any(file => file.LocalPath == localPath)) return;
            Files.Add(new ExportedFileDescription {
                LocalPath = localPath,
                Contents = contents
            });
        }
    }
}