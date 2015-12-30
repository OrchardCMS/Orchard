using Orchard.DynamicForms.Validators.Settings;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Taxonomy : LabeledFormElement {
        
        public string InputType {
            get { return this.Retrieve(x => x.InputType, ()=> "SelectList"); }
            set { this.Store(x => x.InputType, value); }
        }

        public int? TaxonomyId {
            get { return this.Retrieve(x => x.TaxonomyId); }
            set { this.Store(x => x.TaxonomyId, value); }
        }

        public string SortOrder {
            get { return this.Retrieve(x => x.SortOrder); }
            set { this.Store(x => x.SortOrder, value); }
        }

        public string OptionLabel {
            get { return this.Retrieve(x => x.OptionLabel); }
            set { this.Store(x => x.OptionLabel, value); }
        }

        public string TextExpression {
            get { return this.Retrieve(x => x.TextExpression); }
            set { this.Store(x => x.TextExpression, value); }
        }

        public string ValueExpression {
            get { return this.Retrieve(x => x.ValueExpression); }
            set { this.Store(x => x.ValueExpression, value); }
        }

        public string DefaultValue {
            get { return this.Retrieve(x => x.DefaultValue); }
        }

        public EnumerationValidationSettings ValidationSettings {
            get { return Data.GetModel<EnumerationValidationSettings>(""); }
        }
    }
}