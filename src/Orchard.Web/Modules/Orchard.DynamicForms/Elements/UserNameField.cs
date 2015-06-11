namespace Orchard.DynamicForms.Elements {
    public class UserNameField : FormElement {
        public override bool HasEditor {
            get { return false; }
        }

        public override string Name {
            get { return "UserName"; }
        }
    }
}