using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Logging;

namespace Orchard.Pages.Services {
    public interface ISlugConstraint : IRouteConstraint, ISingletonDependency {
        void SetCurrentlyPublishedSlugs(IEnumerable<string> slugs);
        string LookupPublishedSlug(string slug);
        void AddPublishedSlug(string slug);
        void RemovePublishedSlug(string slug);
    }

    public class SlugConstraint : ISlugConstraint {
        /// <summary>
        /// Singleton object, per Orchard Shell instance. We need to protect concurrent access to the
        /// dictionary.
        /// </summary>
        private readonly object _syncLock = new object();
        private IDictionary<string, string> _currentlyPublishedSlugs = new Dictionary<string, string>();

        public SlugConstraint() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void SetCurrentlyPublishedSlugs(IEnumerable<string> values) {
            // Make a copy to avoid performing potential lazy computation inside the lock
            string[] valuesArray = values.ToArray();

            lock (_syncLock) {
                _currentlyPublishedSlugs = valuesArray
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
            }

            Logger.Debug("Publishing slugs: {0}", string.Join(", ", valuesArray));
        }

        public string LookupPublishedSlug(string slug) {
            lock (_syncLock) {
                if (slug == null)
                    return "";

                string actual;
                if (_currentlyPublishedSlugs.TryGetValue(slug, out actual))
                    return actual;
                return slug;
            }
        }

        public void AddPublishedSlug(string slug) {
            lock (_syncLock) {
                _currentlyPublishedSlugs[slug] = slug;
            }
        }

        public void RemovePublishedSlug(string slug) {
            lock (_syncLock) {
                _currentlyPublishedSlugs.Remove(slug);
            }
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);

                lock (_syncLock) {
                    return _currentlyPublishedSlugs.ContainsKey(parameterValue);
                }
            }
            return false;
        }
    }
}