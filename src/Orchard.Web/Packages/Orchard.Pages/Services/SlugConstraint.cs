using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.Pages.Services {
    public interface ISlugConstraint : IRouteConstraint, ISingletonDependency {
        void SetCurrentlyPublishedSlugs(IEnumerable<string> slugs);
        string LookupPublishedSlug(string slug);
    }

    public class SlugConstraint : ISlugConstraint {
        private IDictionary<string, string> _currentlyPublishedSlugs = new Dictionary<string, string>();

        public void SetCurrentlyPublishedSlugs(IEnumerable<string> values) {
            _currentlyPublishedSlugs = values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
        }

        public string LookupPublishedSlug(string slug) {
            string actual;
            if (_currentlyPublishedSlugs.TryGetValue(slug, out actual))
                return actual;
            return slug;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);
                return _currentlyPublishedSlugs.ContainsKey(parameterValue);
            }
            return false;
        }
    }
}