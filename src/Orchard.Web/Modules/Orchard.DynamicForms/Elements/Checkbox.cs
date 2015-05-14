using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class CheckBox : LabeledFormElement {
        public override string ToolboxIcon {
            get { return "\uf046"; }
        }

        public CheckBoxValidationSettings ValidationSettings {
            get { return Data.GetModel<CheckBoxValidationSettings>(""); }
        }
    }
}