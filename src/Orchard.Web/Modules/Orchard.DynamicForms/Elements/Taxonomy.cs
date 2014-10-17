using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Taxonomy : LabeledFormElement {
        
        public string InputType {
            get { return State.Get("InputType", "SelectList"); }
            set { State["InputType"] = value; }
        }

        public int? TaxonomyId {
            get { return State.Get("TaxonomyId").ToInt32(); }
            set { State["TaxonomyId"] = value.ToString(); }
        }

        public string SortOrder {
            get { return State.Get("SortOrder"); }
            set { State["SortOrder"] = value; }
        }

        public string OptionLabel {
            get { return State.Get("OptionLabel"); }
            set { State["OptionLabel"] = value; }
        }

        public string TextExpression {
            get { return State.Get("TextExpression"); }
            set { State["TextExpression"] = value; }
        }

        public string ValueExpression {
            get { return State.Get("ValueExpression"); }
            set { State["ValueExpression"] = value; }
        }
    }
}