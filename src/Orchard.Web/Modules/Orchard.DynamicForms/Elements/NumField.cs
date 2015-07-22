using System;
using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class NumField : LabeledFormElement {
        public NumFieldValidationSettings ValidationSettings {
            get { return Data.GetModel<NumFieldValidationSettings>(""); }
        }
    }

    public class NumFieldNumericModel : NumFieldValidationSettings {
        public Decimal? Value { get; set; }
    }
}