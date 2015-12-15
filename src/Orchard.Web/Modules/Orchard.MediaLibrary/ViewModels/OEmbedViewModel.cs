using System.Xml.Linq;

namespace Orchard.MediaLibrary.ViewModels {
    public class OEmbedViewModel {
        public string FolderPath { get; set; }
        public string Url { get; set; }
        public XDocument Content { get; set; }
        public bool Success { get; set; }
        public string Type { get; set; }
    }
}