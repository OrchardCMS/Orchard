using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Handlers {
    [UsedImplicitly]
    public class WidgetPartHandler : ContentHandler {
        public WidgetPartHandler(IRepository<WidgetPartRecord> widgetsRepository) {
            Filters.Add(StorageFilter.For(widgetsRepository));
        }
    }
}