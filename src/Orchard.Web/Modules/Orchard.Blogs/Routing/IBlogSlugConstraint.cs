using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Blogs.Routing {
    public interface IBlogSlugConstraint : IRouteConstraint, ISingletonDependency {
        void SetSlugs(IEnumerable<string> slugs);
        string FindSlug(string slug);
        void AddSlug(string slug);
        void RemoveSlug(string slug);
    }
}