using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Button : FormElement {
        public override string ToolboxIcon {
            get { return "\uf096"; }
        }

        public string Text {
            get { return this.Retrieve(x => x.Text, () => "Submit"); }
            set { this.Store(x => x.Text, value); }
        }
    }
}