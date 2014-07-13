using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Orchard.Mvc.Routes {

    public class HubRoute : RouteBase, IRouteWithArea, IComparable<HubRoute> {
        private readonly IRunningShellTable _runningShellTable;

        private readonly ConcurrentDictionary<string, IList<RouteBase>> _routesByShell = new ConcurrentDictionary<string, IList<RouteBase>>();

        public HubRoute(string name, string area, int priority, IRunningShellTable runningShellTable) {
            Priority = priority;
            Area = area;
            Name = name;
            _runningShellTable = runningShellTable;
        }

        public string Name { get; private set; }
        public string Area { get; private set; }
        public int Priority { get; private set; }

        /// <summary>
        /// Removes the routes associated with a shell
        /// </summary>
        public void ReleaseShell(ShellSettings shellSettings) {
            IList<RouteBase> routes;
            _routesByShell.TryRemove(shellSettings.Name, out routes);
        }

        public void Add(RouteBase route, ShellSettings shellSettings) {
            var routes = _routesByShell.GetOrAdd(shellSettings.Name, key => new List<RouteBase>());
            routes.Add(route);
        }

        public override RouteData GetRouteData(HttpContextBase httpContext) {
            var settings = _runningShellTable.Match(httpContext);

            if (settings == null)
                return null;

            IList<RouteBase> routes;
            if (!_routesByShell.TryGetValue(settings.Name, out routes)) {
                return null;
            }

            foreach (var route in routes) {
                RouteData routeData = route.GetRouteData(httpContext);
                if(routeData != null) {
                    return routeData;
                }
            }

            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            var settings = _runningShellTable.Match(requestContext.HttpContext);

            if (settings == null)
                return null;

            IList<RouteBase> routes;
            if (!_routesByShell.TryGetValue(settings.Name, out routes)) {
                return null;
            }

            foreach (var route in routes) {
                VirtualPathData virtualPathData = route.GetVirtualPath(requestContext, values);
                if (virtualPathData != null) {
                    return virtualPathData;
                }
            }

            return null;
        }

        public int CompareTo(HubRoute other) {
            if (other == null) {
                return -1;
            }

            if (other == this) {
                return 0;
            }

            if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(other.Name) || Name == other.Name) {
                return 0;
            }

            if (!String.Equals(other.Area, Area, StringComparison.OrdinalIgnoreCase)) {
                return StringComparer.OrdinalIgnoreCase.Compare(other.Area, Area);
            }

            if (other.Priority == Priority) {
                return StringComparer.OrdinalIgnoreCase.Compare(other.Area, Area);
            }

            return Priority.CompareTo(other.Priority);
        }
    }
}
