using System.Web.Mvc;
using Futures.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.Settings;
using Orchard.UI.Admin;

namespace Futures.Widgets {
    public class WidgetFilter : FilterProvider, IActionFilter {
        private readonly IContentManager _contentManager;

        public WidgetFilter(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public virtual ISite CurrentSite { get; set; }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            var model = BaseViewModel.From(filterContext.Result);
            if (model == null || AdminFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            var siteWidgets = CurrentSite.As<WidgetsPart>();
            if (siteWidgets == null) {
                return;
            }

            var zones = model.Zones;
            foreach (var widget in siteWidgets.Widgets) {
                zones.AddDisplayItem(
                    widget.Record.Zone + ":" + widget.Record.Position,
                    _contentManager.BuildDisplayModel(widget, "Widget"));
            }
        }
    }
}
