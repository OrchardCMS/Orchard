using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Label : Element {
        public override string Category {
            get { return "Form"; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public string Text {
            get { return State.Get("LabelText"); }
            set { State["LabelText"] = value; }
        }

        public string For {
            get { return State.Get("LabelFor"); }
            set { State["LabelFor"] = value; }
        }
    }
}