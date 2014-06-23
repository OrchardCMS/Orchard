using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Settings.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Filters {
    public class WidgetFilter : FilterProvider, IResultFilter {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRuleManager _ruleManager;
        private readonly IWidgetsService _widgetsService;
        private readonly IOrchardServices _orchardServices;

        public WidgetFilter(
            IWorkContextAccessor workContextAccessor, 
            IRuleManager ruleManager, 
            IWidgetsService widgetsService,
            IOrchardServices orchardServices) {
            _workContextAccessor = workContextAccessor;
            _ruleManager = ruleManager;
            _widgetsService = widgetsService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; private set; }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // layers and widgets should only run on a full view rendering result
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Layout == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext) ||
                !ThemeFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            IEnumerable<LayerPart> activeLayers = _orchardServices.ContentManager.Query<LayerPart>().ForType("Layer").List();

            var activeLayerIds = new List<int>();
            foreach (var activeLayer in activeLayers) {
                // ignore the rule if it fails to execute
                try {
                    if (_ruleManager.Matches(activeLayer.LayerRule)) {
                        activeLayerIds.Add(activeLayer.ContentItem.Id);
                    }
                }
                catch(Exception e) {
                    Logger.Warning(e, T("An error occured during layer evaluation on: {0}", activeLayer.Name).Text);
                }
            }

            IEnumerable<WidgetPart> widgetParts = _widgetsService.GetWidgets(layerIds: activeLayerIds.ToArray());

            // Build and add shape to zone.
            var zones = workContext.Layout.Zones;
            var defaultCulture = workContext.CurrentSite.As<SiteSettingsPart>().SiteCulture;
            var currentCulture = workContext.CurrentCulture;


            var tasks = widgetParts.Select(async widgetPart => {
                var tuple = new PartAndShape(widgetPart);
                var commonPart = widgetPart.As<ICommonPart>();
                if (commonPart == null || commonPart.Container == null) {
                    Logger.Warning("The widget '{0}' is has no assigned layer or the layer does not exist.", widgetPart.Title);
                    return tuple;
                }

                // ignore widget for different cultures
                var localizablePart = widgetPart.As<ILocalizableAspect>();
                if (localizablePart != null) {
                    // if localized culture is null then show if current culture is the default
                    // this allows a user to show a content item for the default culture only
                    if (localizablePart.Culture == null && defaultCulture != currentCulture) {
                        return tuple;
                    }

                    // if culture is set, show only if current culture is the same
                    if (localizablePart.Culture != null && localizablePart.Culture != currentCulture) {
                        return tuple;
                    }
                }

                // check permissions
                if (!_orchardServices.Authorizer.Authorize(Core.Contents.Permissions.ViewContent, widgetPart)) {
                    return tuple;
                }

                tuple.Shape = await _orchardServices.ContentManager.BuildDisplayAsync(widgetPart);

                return tuple;
            }).ToArray();

            foreach (var task in tasks.InCompletionOrder().Where(t => t.Result.Shape != null)) {
                zones[task.Result.Part.Zone].Add(task.Result.Shape, task.Result.Part.Position);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        private class PartAndShape {
            public PartAndShape(WidgetPart part) {
                Part = part;
            }

            public WidgetPart Part { get; private set; }
            public dynamic Shape { get; set; }
        }
    }
}
