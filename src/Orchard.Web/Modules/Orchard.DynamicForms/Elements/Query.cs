using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Query : LabeledFormElement {
        
        public string InputType {
            get { return State.Get("InputType", "SelectList"); }
            set { State["InputType"] = value; }
        }

        public int? QueryId {
            get { return State.Get("QueryId").ToInt32(); }
            set { State["QueryId"] = value.ToString(); }
        }

        public string OptionLabel {
            get { return State.Get("OptionLabel"); }
            set { State["OptionLabel"] = value; }
        }

        public string TextExpression {
            get { return State.Get("TextExpression", "{Content.Title}"); }
        }

        public string ValueExpression {
            get { return State.Get("ValueExpression", "{Content.Id}"); }
        }
    }
}