using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements
{
    [OrchardFeature("Orchard.Layouts.Snippets")]
    public class Snippet : Element
    {
        public override string Category => "Snippets";

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;

        public override string ToolboxIcon => "\uf10c";
    }
}