using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public abstract class LabeledFormElement : FormElement {
        public string Label {
            get { return State.Get("Label"); }
            set { State["Label"] = value; }
        }

        public bool ShowLabel {
            get { return State.Get("ShowLabel").ToBoolean().GetValueOrDefault(); }
            set { State["ShowLabel"] = value.ToString(); }
        }
    }
}