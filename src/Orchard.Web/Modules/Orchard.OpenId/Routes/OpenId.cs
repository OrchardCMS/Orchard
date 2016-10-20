using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Mvc.Routes;
using Orchard.OpenId.Models;
using Orchard.Settings;

namespace Orchard.Azure.Authentication {
    public class OpenIdRoutes : IRouteProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public OpenIdRoutes(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            if (IsAnyProviderSettingsValid() == false)
                return Enumerable.Empty<RouteDescriptor>();

            return new[] {
                new RouteDescriptor {
                    Priority = 10,
                    Route = new Route(
                        "Users/Account/{action}",
                        new RouteValueDictionary {
                            {"area", "Orchard.OpenId"},
                            {"controller", "Account"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.OpenId"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 10,
                    Route = new Route(
                        "Authentication/Error/",
                        new RouteValueDictionary {
                            {"area", "Orchard.OpenId"},
                            {"controller", "Account"},
                            { "action", "Error" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.OpenId"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        private bool IsAnyProviderSettingsValid() {
            ActiveDirectoryFederationServicesSettingsPart adfsSettings;
            AzureActiveDirectorySettingsPart azureSettings;
            FacebookSettingsPart facebookSettings;
            GoogleSettingsPart googleSettings;
            TwitterSettingsPart twitterSettings;

            using (var scope = _workContextAccessor.CreateWorkContextScope()) {
                var siteSettings = scope.Resolve<ISiteService>().GetSiteSettings();

                adfsSettings = siteSettings.As<ActiveDirectoryFederationServicesSettingsPart>();
                azureSettings = siteSettings.As<AzureActiveDirectorySettingsPart>();
                facebookSettings = siteSettings.As<FacebookSettingsPart>();
                googleSettings = siteSettings.As<GoogleSettingsPart>();
                twitterSettings = siteSettings.As<TwitterSettingsPart>();
            }

            return (
                (adfsSettings != null && adfsSettings.IsValid) ||
                (azureSettings != null && azureSettings.IsValid) ||
                (facebookSettings != null && facebookSettings.IsValid) ||
                (googleSettings != null && googleSettings.IsValid) ||
                (twitterSettings != null && twitterSettings.IsValid)
            );
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var route in GetRoutes()) {
                routes.Add(route);
            }
        }
    }
}