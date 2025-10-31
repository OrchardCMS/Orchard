using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements
{
    public class ContentPart : Element
    {
        public override string Category => "ContentParts";

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;
    }
}