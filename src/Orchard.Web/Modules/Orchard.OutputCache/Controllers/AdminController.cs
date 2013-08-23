using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Features.Metadata;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.OutputCache.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Routes;
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage cache")))
                return new HttpUnauthorizedResult();

            var routeConfigurations = new List<RouteConfiguration>();
            var settings = Services.WorkContext.CurrentSite.As<CacheSettingsPart>();


            foreach (var routeProvider in _routeProviders) {
                // right now, ignore generic routes
                if (routeProvider.Value is StandardExtensionRouteProvider) continue;

                var routeCollection = routeProvider.Value.GetRoutes();
                var feature = routeProvider.Metadata["Feature"] as Orchard.Environment.Extensions.Models.Feature;

                // if there is no feature, skip route
                if (feature == null) continue;

                foreach (var routeDescriptor in routeCollection) {
                    var route = routeDescriptor.Route as Route;

                    if(route == null) {
                        continue;
                    }

                    // ignore admin routes
                    if (route.Url.StartsWith("Admin/") || route.Url == "Admin") continue;

                    var cacheParameterKey = _cacheService.GetRouteDescriptorKey(HttpContext, route);
                    var cacheParameter = _cacheService.GetCacheParameterByKey(cacheParameterKey);
                    var duration = cacheParameter == null ? default(int?) : cacheParameter.Duration;

                    routeConfigurations.Add(new RouteConfiguration {
                        RouteKey = cacheParameterKey,
                        Url = route.Url,
                        Priority = routeDescriptor.Priority,
                        Duration = duration,
                        FeatureName =
                            String.IsNullOrWhiteSpace(feature.Descriptor.Name)
                                ? feature.Descriptor.Id
                                : feature.Descriptor.Name
                    });
                }
            }

            var model = new IndexViewModel {
                DefaultCacheDuration = settings.DefaultCacheDuration,
                DefaultMaxAge = settings.DefaultMaxAge,
                VaryQueryStringParameters = settings.VaryQueryStringParameters,
                VaryRequestHeaders = settings.VaryRequestHeaders,
                IgnoredUrls = settings.IgnoredUrls,
                DebugMode = settings.DebugMode,
                ApplyCulture = settings.ApplyCulture,
                RouteConfigurations = routeConfigurations
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage cache")))
                return new HttpUnauthorizedResult();

            var model = new IndexViewModel {
                RouteConfigurations = new List<RouteConfiguration>()
            };

            if(TryUpdateModel(model)) {
                var settings = Services.WorkContext.CurrentSite.As<CacheSettingsPart>();
                settings.DefaultCacheDuration = model.DefaultCacheDuration;
                settings.DefaultMaxAge = model.DefaultMaxAge;
                settings.VaryQueryStringParameters = model.VaryQueryStringParameters;
                settings.VaryRequestHeaders = model.VaryRequestHeaders;
                settings.IgnoredUrls = model.IgnoredUrls;
                settings.DebugMode = model.DebugMode;
                settings.ApplyCulture = model.ApplyCulture;

                // invalidates the settings cache
                _signals.Trigger(CacheSettingsPart.CacheKey);

                _cacheService.SaveCacheConfigurations(model.RouteConfigurations);

                Services.Notifier.Information(T("Cache Settings saved successfully."));
            }
            else {
                Services.Notifier.Error(T("Could not save Cache Settings."));
            }

            return RedirectToAction("Index");
        }

    }
}