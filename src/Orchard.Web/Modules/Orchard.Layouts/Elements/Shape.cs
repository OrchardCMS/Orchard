using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements
{
    public class Shape : Element
    {
        public override string Category => "Content";

        public override LocalizedString Description => T("Add a shape to your layout.");

        public override string ToolboxIcon => "\uf10c";

        public string ShapeType
        {
            get { return this.Retrieve(x => x.ShapeType); }
            set { this.Store(x => x.ShapeType, value); }
        }
    }
}