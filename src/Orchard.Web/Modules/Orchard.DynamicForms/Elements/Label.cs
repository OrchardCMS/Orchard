using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Label : Element {
        public override string Category {
            get { return "Forms"; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public string Text {
            get { return this.Retrieve<string>("LabelText"); }
            set { this.Store("LabelText", value); }
        }

        public string For {
            get { return this.Retrieve<string>("LabelFor"); }
            set { this.Store("LabelFor", value); }
        }
    }
}