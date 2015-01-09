using System.IO;

namespace Orchard.ContentManagement.Handlers {
    public class ExportedFileDescription {
        public string LocalPath { get; set; }
        public Stream Contents { get; set; }
    }
}
