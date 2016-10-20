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
            AzureActiveDirectorySettingsPart azureSettings;

            using (var scope = _workContextAccessor.CreateWorkContextScope()) {
                azureSettings = scope.Resolve<ISiteService>().GetSiteSettings().As<AzureActiveDirectorySettingsPart>();
            }

            // TODO: Check against other providers after adding site settings for each
            if (azureSettings == null || !azureSettings.IsValid)
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

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var route in GetRoutes()) {
                routes.Add(route);
            }
        }
    }
}