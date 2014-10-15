using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Button : FormElement {
        public override bool HasEditor {
            get { return true; }
        }

        public string Text {
            get { return State.Get("ButtonText", "Submit"); }
            set { State["ButtonText"] = value; }
        }
    }
}