using System;
using System.Collections.Generic;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Services {
    public interface IRoutableService : IDependency {
        void FillSlug<TModel>(TModel model) where TModel : RoutePart;
        void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : RoutePart;
        string GenerateUniqueSlug(RoutePart part, IEnumerable<string> existingPaths);

        /// <summary>
        /// Returns any content item with similar path
        /// </summary>
        IEnumerable<RoutePart> GetSimilarPaths(string path);

        /// <summary>
        /// Validates the given slug
        /// </summary>
        bool IsSlugValid(string slug);

        /// <summary>
        /// Defines the slug of a RoutableAspect and validate its unicity
        /// </summary>
        /// <returns>True if the slug has been created, False if a conflict occured</returns>
        bool ProcessSlug(RoutePart part);

    }
}