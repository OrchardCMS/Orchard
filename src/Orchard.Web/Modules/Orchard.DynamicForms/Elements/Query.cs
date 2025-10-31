using Orchard.DynamicForms.Validators.Settings;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements
{
    public class Query : LabeledFormElement
    {

        public string InputType
        {
            get { return this.Retrieve(x => x.InputType, () => "SelectList"); }
            set { this.Store(x => x.InputType, value); }
        }

        public int? QueryId
        {
            get { return this.Retrieve(x => x.QueryId); }
            set { this.Store(x => x.QueryId, value); }
        }

        public string OptionLabel
        {
            get { return this.Retrieve(x => x.OptionLabel); }
            set { this.Store(x => x.OptionLabel, value); }
        }

        public string TextExpression => this.Retrieve(x => x.TextExpression, () => "{Content.Title}");

        public string ValueExpression => this.Retrieve(x => x.ValueExpression, () => "{Content.Id}");

        public string DefaultValue => this.Retrieve(x => x.DefaultValue);

        public EnumerationValidationSettings ValidationSettings => Data.GetModel<EnumerationValidationSettings>("");
    }
}