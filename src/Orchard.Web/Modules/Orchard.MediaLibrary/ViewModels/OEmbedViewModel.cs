using System.Xml.Linq;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels
{
    public class OEmbedViewModel
    {
        public string FolderPath { get; set; }
        public string Url { get; set; }
        public XDocument Content { get; set; }
        public MediaPart Replace { get; set; }
    }
}