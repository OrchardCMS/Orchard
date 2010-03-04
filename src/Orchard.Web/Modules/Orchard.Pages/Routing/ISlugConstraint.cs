using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Pages.Routing {
    public interface ISlugConstraint : IRouteConstraint, ISingletonDependency {
        void SetCurrentlyPublishedSlugs(IEnumerable<string> slugs);
        string LookupPublishedSlug(string slug);
        void AddPublishedSlug(string slug);
        void RemovePublishedSlug(string slug);
    }
}