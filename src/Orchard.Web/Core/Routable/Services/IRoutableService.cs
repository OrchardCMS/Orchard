using System.Collections.Generic;
using Orchard.ContentManagement.Parts;

namespace Orchard.Core.Routable.Services {
    public interface IRoutableService : IDependency {
        void FillSlugFromTitle<TModel>(TModel model) where TModel : IRoutePart;
        string GenerateUniqueSlug(IRoutePart part, IEnumerable<string> existingPaths);

        /// <summary>
        /// Returns any content item with similar path
        /// </summary>
        IEnumerable<IRoutePart> GetSimilarPaths(string path);

        /// <summary>
        /// Validates the given slug
        /// </summary>
        bool IsSlugValid(string slug);

        /// <summary>
        /// Defines the slug of a RoutePart and validate its unicity
        /// </summary>
        /// <returns>True if the slug has been created, False if a conflict occured</returns>
        bool ProcessSlug(IRoutePart part);

        /// <summary>
        /// Updated the paths of all contained items to reflect the current path of this item
        /// </summary>
        void FixContainedPaths(IRoutePart part);
    }
}