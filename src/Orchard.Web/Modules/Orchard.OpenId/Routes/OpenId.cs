using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Orchard.OpenId.Services;

namespace Orchard.Azure.Authentication {
    public class OpenIdRoutes : IRouteProvider {
        private readonly IEnumerable<IOpenIdProvider> _openIdProviders;

        public OpenIdRoutes(IEnumerable<IOpenIdProvider> openIdProviders) {
            _openIdProviders = openIdProviders;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var route in GetRoutes()) {
                routes.Add(route);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            if (IsAnyProviderSettingsValid() == false)
                return Enumerable.Empty<RouteDescriptor>();

            return new[] {
                new RouteDescriptor {
                    Priority = 11,
                    Route = new Route(
                        "Users/Account/Challenge/{openIdProvider}",
                        new RouteValueDictionary {
                            {"area", "Orchard.OpenId"},
                            {"controller", "Account"},
                            {"action", "Challenge"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.OpenId"},
                            {"controller", "Account"},
                            {"action", "Challenge"}
                        },
                        new MvcRouteHandler())
                },
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
                            {"area", "Orchard.OpenId"},
                            {"controller", "Account"}
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
                            {"area", "Orchard.OpenId"},
                            {"controller", "Account"},
                            { "action", "Error" }
                        },
                        new MvcRouteHandler())
                }
            };
        }

        private bool IsAnyProviderSettingsValid() {
            return _openIdProviders.Any(provider => provider.IsValid);
        }
    }
}