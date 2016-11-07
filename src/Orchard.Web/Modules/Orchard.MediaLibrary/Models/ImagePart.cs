using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.MediaLibrary.Models {
    public class ImagePart : ContentPart {

        public int Width {
            get { return Convert.ToInt32(this.As<InfosetPart>().Get("ImagePart", "Width", "Value"), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("ImagePart", "Width", "Value", Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }

        public int Height {
            get { return Convert.ToInt32(this.As<InfosetPart>().Get("ImagePart", "Height", "Value"), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("ImagePart", "Height", "Value", Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }
   }
}