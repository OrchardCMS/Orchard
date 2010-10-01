using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Filters {
    public class WidgetFilter : FilterProvider, IResultFilter {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public WidgetFilter(IContentManager contentManager, IWorkContextAccessor workContextAccessor) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Page == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            IEnumerable<WidgetPart> widgetParts = _contentManager.Query<WidgetPart, WidgetPartRecord>().List();

            // Build and add shape to zone.
            var zones = workContext.Page.Zones;
            foreach (var widgetPart in widgetParts) {
                var widgetShape = _contentManager.BuildDisplayModel(widgetPart);

                zones[widgetPart.Record.Zone].Add(widgetShape, widgetPart.Record.Position);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}
