using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using IDeliverable.Widgets.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Models;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace IDeliverable.Widgets.Filters
{
    [OrchardFeature("IDeliverable.Widgets.OutputCache")]
    [OrchardSuppressDependency("Orchard.Widgets.Filters.WidgetFilter")]
    public class WidgetFilter : FilterProvider, IResultFilter
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRuleManager _ruleManager;
        private readonly IWidgetsService _widgetsService;
        private readonly IOrchardServices _orchardServices;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IThemeManager _themeManager;
        private readonly ShellSettings _shellSettings;
        private CacheSettings _cacheSettings;

        public WidgetFilter(
            IWorkContextAccessor workContextAccessor,
            IRuleManager ruleManager,
            IWidgetsService widgetsService,
            IOrchardServices orchardServices,
            IShapeDisplay shapeDisplay,
            ICacheManager cacheManager,
            ISignals signals,
            IThemeManager themeManager,
            ShellSettings shellSettings)
        {

            _workContextAccessor = workContextAccessor;
            _ruleManager = ruleManager;
            _widgetsService = widgetsService;
            _orchardServices = orchardServices;
            _shapeDisplay = shapeDisplay;
            _cacheManager = cacheManager;
            _signals = signals;
            _themeManager = themeManager;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; private set; }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // layers and widgets should only run on a full view rendering result
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Layout == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext) ||
                !ThemeFilter.IsApplied(filterContext.RequestContext))
            {
                return;
            }

            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            var activeLayers = _orchardServices.ContentManager.Query<LayerPart, LayerPartRecord>().List();
            var activeLayerIds = new List<int>();

            foreach (var activeLayer in activeLayers)
            {
                // ignore the rule if it fails to execute
                try
                {
                    if (_ruleManager.Matches(activeLayer.Record.LayerRule))
                    {
                        activeLayerIds.Add(activeLayer.ContentItem.Id);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning(e, T("An error occured during layer evaluation on: {0}", activeLayer.Name).Text);
                }
            }

            var widgetParts = _widgetsService.GetWidgets(layerIds: activeLayerIds.ToArray());

            // Build and add shape to zone.
            var zones = workContext.Layout.Zones;
            var defaultCulture = workContext.CurrentSite.As<SiteSettingsPart>().SiteCulture;
            var currentCulture = workContext.CurrentCulture;

            foreach (var widgetPart in widgetParts)
            {
                var commonPart = widgetPart.As<ICommonPart>();
                if (commonPart == null || commonPart.Container == null)
                {
                    Logger.Warning("The widget '{0}' is has no assigned layer or the layer does not exist.", widgetPart.Title);
                    continue;
                }

                // ignore widget for different cultures
                var localizablePart = widgetPart.As<ILocalizableAspect>();
                if (localizablePart != null)
                {
                    // if localized culture is null then show if current culture is the default
                    // this allows a user to show a content item for the default culture only
                    if (localizablePart.Culture == null && defaultCulture != currentCulture)
                    {
                        continue;
                    }

                    // if culture is set, show only if current culture is the same
                    if (localizablePart.Culture != null && localizablePart.Culture != currentCulture)
                    {
                        continue;
                    }
                }

                // check permissions
                if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, widgetPart))
                {
                    continue;
                }

                var useCache = UseCache(widgetPart, filterContext);
                var widgetShape = useCache
                    ? BuildCachedWidgetShape(widgetPart, filterContext)
                    : BuildWidgetShape(widgetPart);

                zones[widgetPart.Record.Zone].Add(widgetShape, widgetPart.Record.Position);
            }
        }

        private static bool UseCache(WidgetPart widgetPart, ControllerContext controllerContext)
        {
            var cachePart = widgetPart.As<OutputCachePart>();
            return cachePart != null && cachePart.Enabled;
        }

        private string ComputeCacheKey(WidgetPart widgetPart, ControllerContext controllerContext)
        {
            var sb = new StringBuilder();
            var workContext = controllerContext.GetWorkContext();
            var theme = _themeManager.GetRequestTheme(controllerContext.RequestContext).Id;
            var url = GetAbsoluteUrl(controllerContext);
            var settings = GetCacheSettings(workContext);
            var varyByHeaders = new HashSet<string>(settings.VaryByRequestHeaders);

            // Different tenants with the same urls have different entries.
            varyByHeaders.Add("HOST");

            var queryString = controllerContext.RequestContext.HttpContext.Request.QueryString;
            var requestHeaders = controllerContext.RequestContext.HttpContext.Request.Headers;
            var parameters = new Dictionary<string, object>();

            foreach (var key in queryString.AllKeys.Where(x => x != null))
            {
                parameters[key] = queryString[key];
            }

            foreach (var header in varyByHeaders)
            {
                if (requestHeaders.AllKeys.Contains(header))
                {
                    parameters["HEADER:" + header] = requestHeaders[header];
                }
            }

            sb.Append("layer=").Append(widgetPart.LayerId.ToString(CultureInfo.InvariantCulture)).Append(";");
            sb.Append("zone=").Append(widgetPart.Zone).Append(";");
            sb.Append("widget=").Append(widgetPart.Id.ToString(CultureInfo.InvariantCulture)).Append(";");
            sb.Append("tenant=").Append(_shellSettings.Name).Append(";");
            sb.Append("url=").Append(url.ToLowerInvariant()).Append(";");

            if (settings.VaryByCulture)
            {
                sb.Append("culture=").Append(workContext.CurrentCulture.ToLowerInvariant()).Append(";");
            }

            sb.Append("theme=").Append(theme.ToLowerInvariant()).Append(";");

            foreach (var pair in parameters)
            {
                sb.AppendFormat("{0}={1};", pair.Key.ToLowerInvariant(), Convert.ToString(pair.Value).ToLowerInvariant());
            }

            return sb.ToString();
        }

        private static string GetAbsoluteUrl(ControllerContext controllerContext)
        {
            var request = controllerContext.RequestContext.HttpContext.Request;
            var url = String.Concat(request.ToApplicationRootUrlString(), request.Path);
            return url;
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        private dynamic BuildWidgetShape(WidgetPart widgetPart)
        {
            return _orchardServices.ContentManager.BuildDisplay(widgetPart);
        }

        private dynamic BuildCachedWidgetShape(WidgetPart widgetPart, ControllerContext controllerContext)
        {
            var cacheKey = ComputeCacheKey(widgetPart, controllerContext);
            var widgetOutput = _cacheManager.Get(cacheKey, context =>
            {
                context.Monitor(_signals.When(OutputCachePart.GenericSignalName));
                context.Monitor(_signals.When(OutputCachePart.ContentSignalName(widgetPart.Id)));
                context.Monitor(_signals.When(OutputCachePart.TypeSignalName(widgetPart.ContentItem.ContentType)));
                var output = RenderWidget(widgetPart);
                return output;
            });
            return _orchardServices.New.RawOutput(Content: widgetOutput);
        }

        private string RenderWidget(IContent widget)
        {
            var widgetShape = _orchardServices.ContentManager.BuildDisplay(widget);
            return _shapeDisplay.Display(widgetShape);
        }

        private CacheSettings GetCacheSettings(WorkContext workContext)
        {
            return _cacheSettings ?? (_cacheSettings = _cacheManager.Get(CacheSettings.CacheKey, context =>
            {
                context.Monitor(_signals.When(CacheSettings.CacheKey));
                return new CacheSettings(workContext.CurrentSite.As<CacheSettingsPart>());
            }));
        }
    }
}