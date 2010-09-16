using System.Web.Mvc;
using Futures.Widgets.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.Settings;
using Orchard.UI.Admin;

namespace Futures.Widgets {
    public class WidgetFilter : FilterProvider, IActionFilter {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public WidgetFilter(IContentManager contentManager, IWorkContextAccessor workContextAccessor) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Page == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            var siteWidgets = workContext.CurrentSite.As<WidgetsPart>();
            if (siteWidgets == null) {
                return;
            }

            var zones = workContext.Page.Zones;
            foreach (var widget in siteWidgets.Widgets) {
                var widgetShape = _contentManager.BuildDisplayModel(widget);

                zones[widget.Record.Zone].Add(widgetShape, widget.Record.Position);
            }
        }
    }
}
