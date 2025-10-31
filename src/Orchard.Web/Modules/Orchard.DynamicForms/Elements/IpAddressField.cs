namespace Orchard.DynamicForms.Elements
{
    public class IpAddressField : FormElement
    {
        public override bool HasEditor => false;

        public override string Name => "IPAddress";
    }
}