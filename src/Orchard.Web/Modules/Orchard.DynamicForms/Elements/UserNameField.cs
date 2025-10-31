namespace Orchard.DynamicForms.Elements
{
    public class UserNameField : FormElement
    {
        public override bool HasEditor => false;

        public override string Name => "UserName";
    }
}