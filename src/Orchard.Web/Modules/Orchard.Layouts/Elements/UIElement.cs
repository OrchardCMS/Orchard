using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements
{
    public abstract class UIElement : Element
    {
        public override string Category => "UI";

        public override string ToolboxIcon => "\uf0c8";
    }
}