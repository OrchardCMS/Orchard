﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Mvc.Routes {
    public class StandardExtensionRouteProvider : IRouteProvider {
        private readonly ShellBlueprint _blueprint;

        public StandardExtensionRouteProvider(ShellBlueprint blueprint) {
            _blueprint = blueprint;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            var displayPathsPerArea = _blueprint.Controllers.GroupBy(
                x => x.AreaName,
                x => x.Feature.Descriptor.Extension);

            foreach (var item in displayPathsPerArea) {
                var areaName = item.Key;
                var extensionDescriptor = item.Distinct().Single();
                var displayPath = extensionDescriptor.Path;
                SessionStateBehavior defaultSessionState;
                Enum.TryParse(extensionDescriptor.SessionState, true /*ignoreCase*/, out defaultSessionState);


                yield return new RouteDescriptor {
                    Priority = -10,
                    SessionState = defaultSessionState, 
                    Route = new Route(
                        "Admin/" + displayPath + "/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "admin"},
                            {"action", "index"},
                            {"id", ""}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", areaName}
                        },
                        new MvcRouteHandler())
                };

                yield return new RouteDescriptor {
                    Priority = -10,
                    SessionState = defaultSessionState,
                    Route = new Route(
                        displayPath + "/{controller}/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "home"},
                            {"action", "index"},
                            {"id", ""}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", areaName}
                        },
                        new MvcRouteHandler())
                };
            }
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}