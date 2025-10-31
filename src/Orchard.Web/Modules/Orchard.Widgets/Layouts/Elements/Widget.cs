using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Widgets.Layouts.Elements
{
    [OrchardFeature("Orchard.Widgets.Elements")]
    public class Widget : Element
    {
        public override string Category => "Widgets";

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;

        public int? WidgetId
        {
            get { return this.Retrieve(x => x.WidgetId); }
            set { this.Store(x => x.WidgetId, value); }
        }
    }
}