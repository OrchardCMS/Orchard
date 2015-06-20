using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Heading : ContentElement {
        
        public override string Category {
            get { return "Content"; }
        }

        public override LocalizedString DisplayText {
            get { return T("Heading h1-h6"); }
        }

        public override string ToolboxIcon {
            get { return "\uf1dc"; }
        }

        public int Level {
            get { return this.Retrieve(h => h.Level); }
            set { this.Store(h => h.Level, value);}
        }
    }
}