using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;

namespace Orchard.Widgets.Filters {
    public class WidgetFilter : FilterProvider, IResultFilter {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public WidgetFilter(IContentManager contentManager, IWorkContextAccessor workContextAccessor) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Page == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            // Get Layers
            // Get LayerZones
            // Get WidgetParts
            // BuildDisplayModel
            // Add to Zone.
        }
    }
}
