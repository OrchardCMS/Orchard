using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;

namespace Futures.Widgets.Models {
    public class WidgetsPart : ContentPart<WidgetsPartRecord> {
        public LazyField<IList<WidgetPart>> WidgetField = new LazyField<IList<WidgetPart>>();
        public IList<WidgetPart> Widgets { get { return WidgetField.Value; } set { WidgetField.Value = value; } }
    }
}
