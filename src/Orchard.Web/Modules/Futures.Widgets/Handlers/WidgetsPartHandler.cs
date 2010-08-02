using System.Linq;
using Futures.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Futures.Widgets.Handlers {
    public class WidgetsPartHandler : ContentHandler {
        public WidgetsPartHandler(
            IRepository<WidgetsPartRecord> widgetsRepository,
            IRepository<WidgetPartRecord> widgetRepository) {

            Filters.Add(new ActivatingFilter<WidgetsPart>("Site"));

            // providing standard storage support for widget records
            Filters.Add(StorageFilter.For(widgetsRepository));
            Filters.Add(StorageFilter.For(widgetRepository));

            OnLoaded<WidgetsPart>(
                (ctx, part) => part.WidgetField.Loader(
                                   () => ctx.ContentManager
                                             .Query<WidgetPart, WidgetPartRecord>()
                                             .Where(x => x.Scope == part.Record)
                                             .List().ToList()));
        }
    }
}
