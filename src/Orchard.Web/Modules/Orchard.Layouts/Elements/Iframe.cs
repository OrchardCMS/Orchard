using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Iframe : ContentElement {
        public override string Category {
            get { return "Content"; }
        }

        public override LocalizedString DisplayText {
            get { return T("Iframe"); }
        }

        public override string ToolboxIcon {
            get { return "\uf121"; }
        }

        public string Src {
            get { return this.Retrieve(x => x.Src); }
            set { this.Store(x => x.Src, value); }
        }
        public int? Width {
            get { return this.Retrieve(x => x.Width); }
            set { this.Store(x => x.Width, value); }
        }
        public int? Height {
            get { return this.Retrieve(x => x.Height); }
            set { this.Store(x => x.Height, value); }
        }
        public bool AllowFullscreen {
            get { return this.Retrieve(x => x.AllowFullscreen); }
            set { this.Store(x => x.AllowFullscreen, value); }
        }
    }
}