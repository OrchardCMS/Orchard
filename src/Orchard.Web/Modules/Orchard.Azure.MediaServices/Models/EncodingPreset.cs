using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Azure.MediaServices.Models {
    public class EncodingPreset {
        public string Name { get; set; }
        [AllowHtml]
        public string CustomXml { get; set; }
    }
}