using System.Linq;
using System.Web.Routing;
using Futures.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Futures.Widgets.Controllers {
    public class WidgetHandler : ContentHandler {
        public WidgetHandler(
            IRepository<HasWidgetsRecord> hasWidgetRepository,
            IRepository<WidgetRecord> widgetRepository) {

            // marking the "site" content type as a widget container
            Filters.Add(new ActivatingFilter<HasWidgets>("site"));

            // adding parts to the "HtmlWidget" content type
            Filters.Add(new ActivatingFilter<Widget>("HtmlWidget"));
            Filters.Add(new ActivatingFilter<BodyAspect>("HtmlWidget"));

            // providing standard storage support for widget records
            Filters.Add(StorageFilter.For(hasWidgetRepository));
            Filters.Add(StorageFilter.For(widgetRepository));

            OnLoaded<HasWidgets>(
                (ctx, part) => part.WidgetField.Loader(
                                   () => ctx.ContentManager
                                             .Query<Widget, WidgetRecord>()
                                             .Where(x => x.Scope == part.Record)
                                             .List().ToList()));
        }
    }

    public class WidgetDriver : ContentItemDriver<Widget> {
        protected override RouteValueDictionary GetEditorRouteValues(Widget item) {
            return new RouteValueDictionary {
                {"Area", "Futures.Widgets"},
                {"Controller", "Admin"},
                {"Action", "Edit"},
                {"Id", item.ContentItem.Id}
            };
        }

        protected override bool UseDefaultTemplate {
            get { return true; }
        }
    }
}
