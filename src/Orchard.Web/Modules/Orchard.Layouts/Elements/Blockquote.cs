using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Blockquote : ContentElement {
        
        public override string Category {
            get { return "Content"; }
        }

        public override LocalizedString DisplayText {
            get { return T("Blockquote"); }
        }

        public override string ToolboxIcon {
            get { return "\uf10d"; }
        }

        public string Citation {
            get { return this.Retrieve(x => x.Citation); }
            set { this.Store(x => x.Citation, value); }
        }
    }
}