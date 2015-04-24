namespace Orchard.DynamicForms.Elements {
    public class IpAddressField : FormElement {
        public override bool HasEditor {
            get { return false; }
        }

        public override string Name {
            get { return "IPAddress"; }
        }
    }
}