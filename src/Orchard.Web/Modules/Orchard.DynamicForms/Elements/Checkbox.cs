using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements
{
    public class CheckBox : LabeledFormElement
    {
        public override string ToolboxIcon => "\uf046";

        public CheckBoxValidationSettings ValidationSettings => Data.GetModel<CheckBoxValidationSettings>("");
    }
}