using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements
{
    public class ContentField : Element
    {
        public override string Category => "ContentFields";

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;
    }
}