using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Logging;

namespace Contrib.Taxonomies.Routing {
    [UsedImplicitly]
    public class TermPathConstraint : ITermPathConstraint {
        /// <summary>
        /// Singleton object, per Orchard Shell instance. We need to protect concurrent access to the dictionary.
        /// </summary>
        private readonly object _syncLock = new object();
        private IDictionary<string, string> _paths = new Dictionary<string, string>();

        public TermPathConstraint() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void SetPaths(IEnumerable<string> paths) {
            // Make a copy to avoid performing potential lazy computation inside the lock
            var pathsArray = paths.Where(p => p != null).ToArray();

            lock (_syncLock) {
                _paths = pathsArray.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
            }

            Logger.Debug("Taxonomy paths: {0}", string.Join(", ", pathsArray));
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