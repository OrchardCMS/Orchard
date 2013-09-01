using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class JavaScriptAntiSpamPart : ContentPart {
        public bool IAmHuman { get; set; }
    }
}