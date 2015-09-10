using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Layouts.ViewModels {
    public class IframeEditorViewModel {
        public string Src { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public bool AllowFullscreen { get; set; }
    }
}