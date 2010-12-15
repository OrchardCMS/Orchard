using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Logging;

namespace Orchard.Core.Routable.Services {
    [UsedImplicitly]
    public class RoutablePathConstraint : IRoutablePathConstraint {
        /// <summary>
        /// Singleton object, per Orchard Shell instance. We need to protect concurrent access to the dictionary.
        /// </summary>
        private readonly object _syncLock = new object();
        private IDictionary<string, string> _paths = new Dictionary<string, string>();

        public RoutablePathConstraint() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void SetPaths(IEnumerable<string> paths) {
            // Make a copy to avoid performing potential lazy computation inside the lock
            var slugsArray = paths.ToArray();

            lock (_syncLock) {
                _paths = slugsArray.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public string FindPath(string path) {
            lock (_syncLock) {
                string actual;
                return _paths.TryGetValue(path, out actual) ? actual : path;
            }
        }

        public void AddPath(string path) {
            lock (_syncLock) {
                _paths[path] = path;
            }
        }

        public void RemovePath(string path) {
            lock (_syncLock) {
                if (path != null && _paths.ContainsKey(path))
                    _paths.Remove(path);
            }
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);

                lock (_syncLock) {
                    return _paths.ContainsKey(parameterValue);
                }
            }

            return false;
        }
    }
}