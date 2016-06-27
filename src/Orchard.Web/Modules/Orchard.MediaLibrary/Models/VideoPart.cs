using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.MediaLibrary.Models {
    public class VideoPart : ContentPart {
        public int Length {
            get { return Convert.ToInt32(this.As<InfosetPart>().Get("VideoPart", "Length", "Value"), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("VideoPart", "Length", "Value", Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }
    }
}