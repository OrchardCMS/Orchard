using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Features.Metadata;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Routes;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.OutputCache.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.OutputCache.Controllers {
    [Admin]
    public class AdminController : Controller {
        private readonly IEnumerable<Meta<IRouteProvider>> _routeProviders;
        private readonly ISignals _signals;
        private readonly ICacheService _cacheService;

        public AdminController(
            IOrchardServices services,
            IEnumerable<Meta<IRouteProvider>> routeProviders,
            ISignals signals,
            ICacheService cacheService) {
            _routeProviders = routeProviders;
            _signals = signals;
            _cacheService = cacheService;
            Services = services;
            }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You do not have permission to manage output cache.")))
                return new HttpUnauthorizedResult();

            var routeConfigs = new List<CacheRouteConfig>();
            var settings = Services.WorkContext.CurrentSite.As<CacheSettingsPart>();

            foreach (var routeProvider in _routeProviders) {
                // Right now, ignore generic routes.
                if (routeProvider.Value is StandardExtensionRouteProvider) continue;

                var routes = new List<RouteDescriptor>();
                routeProvider.Value.GetRoutes(routes);
                var feature = routeProvider.Metadata["Feature"] as Orchard.Environment.Extensions.Models.Feature;

                // If there is no feature, skip route.
                if (feature == null) continue;

                foreach (var routeDescriptor in routes) {
                    var route = routeDescriptor.Route as Route;

                    if(route == null) {
                        continue;
                    }

                    // Ignore admin routes.
                    if (route.Url.StartsWith("Admin/") || route.Url == "Admin") continue;

                    var cacheParameterKey = _cacheService.GetRouteDescriptorKey(HttpContext, route);
                    var cacheParameter = _cacheService.GetCacheParameterByKey(cacheParameterKey);
                    var duration = cacheParameter == null ? default(int?) : cacheParameter.Duration;
                    var graceTime = cacheParameter == null ? default(int?) : cacheParameter.GraceTime;

                    routeConfigs.Add(new CacheRouteConfig {
                        RouteKey = cacheParameterKey,
                        Url = route.Url,
                        Priority = routeDescriptor.Priority,
                        Duration = duration,
                        GraceTime = graceTime,
                        FeatureName =
                            String.IsNullOrWhiteSpace(feature.Descriptor.Name)
                                ? feature.Descriptor.Id
                                : feature.Descriptor.Name
                    });
                }
            }

            var model = new IndexViewModel {
                RouteConfigs = routeConfigs,
                DefaultCacheDuration = settings.DefaultCacheDuration,
                DefaultCacheGraceTime = settings.DefaultCacheGraceTime,
                DefaultMaxAge = settings.DefaultMaxAge,
                VaryByQueryStringIsExclusive = settings.VaryByQueryStringIsExclusive,
                VaryByQueryStringParameters = settings.VaryByQueryStringParameters,
                VaryByRequestHeaders = settings.VaryByRequestHeaders,
                VaryByRequestCookies = settings.VaryByRequestCookies,
                IgnoredUrls = settings.IgnoredUrls,
                IgnoreNoCache = settings.IgnoreNoCache,
                VaryByCulture = settings.VaryByCulture,
                CacheAuthenticatedRequests = settings.CacheAuthenticatedRequests,
                VaryByAuthenticationState = settings.VaryByAuthenticationState,
                DebugMode = settings.DebugMode
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You do not have permission to manage output cache.")))
                return new HttpUnauthorizedResult();

            var model = new IndexViewModel {
                RouteConfigs = new List<CacheRouteConfig>()
            };

            if(TryUpdateModel(model)) {
                var settings = Services.WorkContext.CurrentSite.As<CacheSettingsPart>();
                settings.DefaultCacheDuration = model.DefaultCacheDuration;
                settings.DefaultCacheGraceTime = model.DefaultCacheGraceTime;
                settings.DefaultMaxAge = model.DefaultMaxAge;
                settings.VaryByQueryStringIsExclusive = model.VaryByQueryStringIsExclusive;
                settings.VaryByQueryStringParameters = model.VaryByQueryStringParameters;
                settings.VaryByRequestHeaders = model.VaryByRequestHeaders;
                settings.VaryByRequestCookies = model.VaryByRequestCookies;
                settings.IgnoredUrls = model.IgnoredUrls;
                settings.IgnoreNoCache = model.IgnoreNoCache;
                settings.VaryByCulture = model.VaryByCulture;
                settings.CacheAuthenticatedRequests = model.CacheAuthenticatedRequests;
                settings.VaryByAuthenticationState = model.VaryByAuthenticationState;
                settings.DebugMode = model.DebugMode;

                // Invalidate the settings cache.
                _signals.Trigger(CacheSettings.CacheKey);
                _cacheService.SaveRouteConfigs(model.RouteConfigs);

                Services.Notifier.Success(T("Output cache settings saved successfully."));
            }
            else {
                Services.Notifier.Error(T("Could not save output cache settings."));
            }

            return RedirectToAction("Index");
        }

    }
}