using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Logging;

namespace Contrib.Taxonomies.Routing {
    [UsedImplicitly]
    public class TaxonomySlugConstraint : ITaxonomySlugConstraint {
        /// <summary>
        /// Singleton object, per Orchard Shell instance. We need to protect concurrent access to the dictionary.
        /// </summary>
        private readonly object _syncLock = new object();
        private IDictionary<string, string> _slugs = new Dictionary<string, string>();

        public TaxonomySlugConstraint() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void SetSlugs(IEnumerable<string> slugs) {
            // Make a copy to avoid performing potential lazy computation inside the lock
            var slugsArray = slugs.ToArray();

            lock (_syncLock) {
                _slugs = slugsArray.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
            }

            Logger.Debug("Taxonomy slugs: {0}", string.Join(", ", slugsArray));
        }

        public string FindSlug(string slug) {
            lock (_syncLock) {
                string actual;
                return _slugs.TryGetValue(slug, out actual) ? actual : slug;
            }
        }

        public void AddSlug(string slug) {
            lock (_syncLock) {
                _slugs[slug] = slug;
            }
        }

        public void RemoveSlug(string slug) {
            lock (_syncLock) {
                _slugs.Remove(slug);
            }
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);

                lock (_syncLock) {
                    return _slugs.ContainsKey(parameterValue);
                }
            }

            return false;
        }
    }
}