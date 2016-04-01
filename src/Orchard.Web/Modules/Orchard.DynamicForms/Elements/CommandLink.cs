using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    
    public enum DynamicFormCommand { New, Delete, First, Previous, Next, Last };

    public class CommandLink : FormElement {
        public DynamicFormCommand DynamicFormCommand {
            get { return (DynamicFormCommand)this.Retrieve<int>("DynamicFormCommand", () => (int) DynamicFormCommand.New); }
            set { this.Store("DynamicFormCommand", value); }
        }
    }
}