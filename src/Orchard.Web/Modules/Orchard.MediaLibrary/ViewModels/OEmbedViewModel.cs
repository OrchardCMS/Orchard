using System.Xml.Linq;

namespace Orchard.MediaLibrary.ViewModels {
    public class OEmbedViewModel {
        public int Id { get; set; }
        public string Url { get; set; }
        public XDocument Content { get; set; }
        public bool Success { get; set; }
    }
}