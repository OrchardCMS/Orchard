using System.Collections.Generic;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Core.Routable.Services {
    public interface IRoutableService : IDependency {
        void FillSlugFromTitle<TModel>(TModel model) where TModel : IRoutableAspect;
        string GenerateUniqueSlug(IRoutableAspect part, IEnumerable<string> existingPaths);

        /// <summary>
        /// Returns any content item with similar path
        /// </summary>
        IEnumerable<IRoutableAspect> GetSimilarPaths(string path);

        /// <summary>
        /// Validates the given slug
        /// </summary>
        bool IsSlugValid(string slug);

        /// <summary>
        /// Defines the slug of a RoutableAspect and validate its unicity
        /// </summary>
        /// <returns>True if the slug has been created, False if a conflict occured</returns>
        bool ProcessSlug(IRoutableAspect part);

        /// <summary>
        /// Updated the paths of all contained items to reflect the current path of this item
        /// </summary>
        void FixContainedPaths(IRoutableAspect part);
    }
}