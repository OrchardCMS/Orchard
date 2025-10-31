using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements
{
    public class RadioButton : LabeledFormElement
    {
        public override string ToolboxIcon => "\uf192";
        public bool DefaultValue
        {
            get { return this.Retrieve(x => x.DefaultValue); }
            set { this.Store(x => x.DefaultValue, value); }
        }
    }
}