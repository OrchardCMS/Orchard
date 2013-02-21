using System.Drawing;
using System.Drawing.Imaging;

namespace Orchard.MediaProcessing.Descriptors.Filter {
    public class FilterContext {
        public dynamic State { get; set; }
        public Image Image { get; set; }
        public ImageFormat ImageFormat { get; set; }
        public string FilePath { get; set; }
        public bool Saved { get; set; }
    }
}