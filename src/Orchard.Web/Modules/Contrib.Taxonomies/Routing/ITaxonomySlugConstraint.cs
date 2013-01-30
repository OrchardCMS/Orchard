using System.Collections.Generic;
using System.Web.Routing;
using Orchard;

namespace Contrib.Taxonomies.Routing {
    public interface ITaxonomySlugConstraint : IRouteConstraint, ISingletonDependency {
        void SetSlugs(IEnumerable<string> slugs);
        string FindSlug(string slug);
        void AddSlug(string slug);
        void RemoveSlug(string slug);
    }
}