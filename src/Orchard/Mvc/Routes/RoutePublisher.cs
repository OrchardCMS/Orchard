using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Castle.Core.Internal;
using Orchard.Environment;
using Orchard.Environment.Configuration;
﻿using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Mvc.Routes {
    public class RoutePublisher : IRoutePublisher {
        private readonly RouteCollection _routeCollection;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IExtensionManager _extensionManager;

        public RoutePublisher(
            RouteCollection routeCollection,
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IRunningShellTable runningShellTable,
            IExtensionManager extensionManager) {
            _routeCollection = routeCollection;
            _shellSettings = shellSettings;
            _workContextAccessor = workContextAccessor;
            _runningShellTable = runningShellTable;
            _extensionManager = extensionManager;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes, Func<IDictionary<string, object>, Task> env) {
            var routesArray = routes
                .OrderByDescending(r => r.Priority)
                .ToArray();

            // this is not called often, but is intended to surface problems before
            // the actual collection is modified
            var preloading = new RouteCollection();
            foreach (var routeDescriptor in routesArray) {

                // extract the WebApi route implementation
                var httpRouteDescriptor = routeDescriptor as HttpRouteDescriptor;
                if (httpRouteDescriptor != null) {
                    var httpRouteCollection = new RouteCollection();
                    httpRouteCollection.MapHttpRoute(httpRouteDescriptor.Name, httpRouteDescriptor.RouteTemplate, httpRouteDescriptor.Defaults, httpRouteDescriptor.Constraints);
                    routeDescriptor.Route = httpRouteCollection.First();
                }

                preloading.Add(routeDescriptor.Name, routeDescriptor.Route);
            }


            using (_routeCollection.GetWriteLock()) {
                // existing routes are removed while the collection is briefly inaccessable
                _routeCollection
                    .OfType<HubRoute>()
                    .ForEach(x => x.ReleaseShell(_shellSettings));

                // HACK: For inserting names in internal dictionary when inserting route to RouteCollection.
                var routeCollectionType = typeof (RouteCollection);
                var namedMap = (Dictionary<string, RouteBase>) routeCollectionType.GetField("_namedMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_routeCollection);

            // new routes are added
                foreach (var routeDescriptor in routesArray) {
                    // Loading session state information. 
                    var defaultSessionState = SessionStateBehavior.Default;

                    ExtensionDescriptor extensionDescriptor = null;
                    if (routeDescriptor.Route is Route) {
                        object extensionId;
                        var route = routeDescriptor.Route as Route;
                        if (route.DataTokens != null && route.DataTokens.TryGetValue("area", out extensionId) ||
                           route.Defaults != null && route.Defaults.TryGetValue("area", out extensionId)) {
                            extensionDescriptor = _extensionManager.GetExtension(extensionId.ToString());
                        }
                    }
                    else if (routeDescriptor.Route is IRouteWithArea) {
                        var route = routeDescriptor.Route as IRouteWithArea;
                        extensionDescriptor = _extensionManager.GetExtension(route.Area);
                    }

                    if (extensionDescriptor != null) {
                        // if session state is not define explicitly, use the one define for the extension
                        if (routeDescriptor.SessionState == SessionStateBehavior.Default) {
                            Enum.TryParse(extensionDescriptor.SessionState, true /*ignoreCase*/, out defaultSessionState);
                        }
                    }

                    // Route-level setting overrides module-level setting (from manifest).
                    var sessionStateBehavior = routeDescriptor.SessionState == SessionStateBehavior.Default ? defaultSessionState : routeDescriptor.SessionState;

                    var shellRoute = new ShellRoute(routeDescriptor.Route, _shellSettings, _workContextAccessor, _runningShellTable, env) {
                        IsHttpRoute = routeDescriptor is HttpRouteDescriptor,
                        SessionState = sessionStateBehavior
                    };

                    var area = extensionDescriptor == null ? "" : extensionDescriptor.Id;

                    var matchedHubRoute = _routeCollection.FirstOrDefault(x => {
                        var hubRoute = x as HubRoute;
                        if (hubRoute == null) {
                            return false;
                        }

                        return routeDescriptor.Priority == hubRoute.Priority && hubRoute.Area.Equals(area, StringComparison.OrdinalIgnoreCase) && hubRoute.Name == routeDescriptor.Name;
                    }) as HubRoute;

                    if (matchedHubRoute == null) {
                        matchedHubRoute = new HubRoute(routeDescriptor.Name, area, routeDescriptor.Priority, _runningShellTable);

                        int index;
                        for (index = 0; index < _routeCollection.Count; index++) {
                            var hubRoute = _routeCollection[index] as HubRoute;
                            if (hubRoute == null) {
                                continue;
                            }
                            if (hubRoute.Priority < matchedHubRoute.Priority) {
                                break;
                            }
                        }
                        
                        _routeCollection.Insert(index, matchedHubRoute);

                        // HACK: For inserting names in internal dictionary when inserting route to RouteCollection.
                        if (!string.IsNullOrEmpty(matchedHubRoute.Name) && !namedMap.ContainsKey(matchedHubRoute.Name)) {
                            namedMap[matchedHubRoute.Name] = matchedHubRoute;
                        }
                    }

                    matchedHubRoute.Add(shellRoute, _shellSettings);
                }
            }
        }
    }
}