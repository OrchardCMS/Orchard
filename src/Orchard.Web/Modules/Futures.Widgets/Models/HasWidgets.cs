using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;

namespace Futures.Widgets.Models {
    public class HasWidgets : ContentPart<HasWidgetsRecord> {
        public LazyField<IList<Widget>> WidgetField = new LazyField<IList<Widget>>();
        public IList<Widget> Widgets { get { return WidgetField.Value; } set { WidgetField.Value = value; } }
    }

    public class HasWidgetsRecord : ContentPartRecord {
    }
}
