using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.Core.Containers {
    public interface IContainersPathConstraint : IRouteConstraint, ISingletonDependency {
        void SetPaths(IEnumerable<string> paths);
        string FindPath(string path);
        void AddPath(string path);
    }

    public class ContainersPathConstraint : IContainersPathConstraint {
        private IDictionary<string, string> _paths = new Dictionary<string, string>();

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);
                return _paths.ContainsKey(parameterValue);
            }

            return false;
        }

        public void SetPaths(IEnumerable<string> paths) {
            // Note: this does not need to be synchronized as long as the dictionary itself is treated as immutable.
            // do not add or remove to the dictionary instance once created. recreate and reassign instead.
            _paths = paths.Distinct().ToDictionary(path => path, StringComparer.OrdinalIgnoreCase);
        }

        public string FindPath(string path) {
            string actual;
            return _paths.TryGetValue(path, out actual) ? actual : path;
        }

        public void AddPath(string path) {
            SetPaths(_paths.Keys.Concat(new[] { path }));
        }
    }
}
