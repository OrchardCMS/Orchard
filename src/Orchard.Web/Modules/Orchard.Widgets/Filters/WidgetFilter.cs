using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;
using Orchard.UI.Widgets;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Filters {
    public class WidgetFilter : FilterProvider, IResultFilter {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRuleManager _ruleManager;

        public WidgetFilter(IContentManager contentManager, IWorkContextAccessor workContextAccessor, IRuleManager ruleManager) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _ruleManager = ruleManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Layout == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            IEnumerable<WidgetPart> widgetParts = _contentManager.Query<WidgetPart, WidgetPartRecord>().List();
            IEnumerable<LayerPart> activeLayers = _contentManager.Query<LayerPart, LayerPartRecord>().List();

            List<int> activeLayerIds = new List<int>();
            foreach (var activeLayer in activeLayers) {
                if (_ruleManager.Matches(activeLayer.Record.LayerRule)) {
                    activeLayerIds.Add(activeLayer.ContentItem.Id);
                }
            }

            // Build and add shape to zone.
            var zones = workContext.Layout.Zones;
            foreach (var widgetPart in widgetParts) {
                if (activeLayerIds.Contains(widgetPart.As<ICommonPart>().Container.ContentItem.Id)) {
                    var widgetShape = _contentManager.BuildDisplayModel(widgetPart);
                    zones[widgetPart.Record.Zone].Add(widgetShape, widgetPart.Record.Position);
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}
