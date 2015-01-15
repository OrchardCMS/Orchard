using Orchard.FileSystems.Media;

namespace Orchard.ContentManagement.Handlers {
    public class ExportedFileDescription {
        public string LocalPath { get; set; }
        public IStorageFile Contents { get; set; }
    }
}
