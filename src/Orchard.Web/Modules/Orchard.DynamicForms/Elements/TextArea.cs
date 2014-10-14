using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class TextArea : LabeledFormElement {
        public int? Rows {
            get { return State.Get("Rows").ToInt32(); }
            set { State["Rows"] = value.ToString(); }
        }

        public int? Columns {
            get { return State.Get("Columns").ToInt32(); }
            set { State["Columns"] = value.ToString(); }
        }
    }
}