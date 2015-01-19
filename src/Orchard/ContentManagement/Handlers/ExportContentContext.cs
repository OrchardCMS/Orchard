using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.FileSystems.Media;

namespace Orchard.ContentManagement.Handlers {
    public class ExportContentContext : ContentContextBase {
        private readonly IList<ExportedFileDescription> _files;

        public XElement Data { get; set; }
        public IEnumerable<ExportedFileDescription> Files { get { return _files; } }

        /// <summary>
        /// Wether the content item should be exclude from the export or not
        /// </summary>
        public bool Exclude { get; set; }

        protected ExportContentContext(ContentItem contentItem) : base(contentItem) {
            _files = new List<ExportedFileDescription>();
        }

        public ExportContentContext(ContentItem contentItem, XElement data)
            : this(contentItem) {
            Data = data;
        }

        public ExportContentContext(ContentItem contentItem, XElement data, IList<ExportedFileDescription> files)
            : base(contentItem) {
            Data = data;
            _files = files;
        }

        public XElement Element(string elementName) {
            var element = Data.Element(elementName);
            if (element == null) {
                element = new XElement(elementName);
                Data.Add(element);
            }
            return element;
        }

        public void AddFile(string localPath, IStorageFile contents) {
            if (_files.Any(file => file.LocalPath == localPath)) return;
            _files.Add(new ExportedFileDescription {
                LocalPath = localPath,
                Contents = contents
            });
        }
    }
}