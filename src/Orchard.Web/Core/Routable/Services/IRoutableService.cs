using System;
using System.Collections.Generic;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Services {
    public interface IRoutableService : IDependency {
        void FillSlug<TModel>(TModel model) where TModel : IsRoutable;
        void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : IsRoutable;
        string GenerateUniqueSlug(string slugCandidate, IEnumerable<string> existingSlugs);

        /// <summary>
        /// Returns any content item of the specified content type with similar slugs
        /// </summary>
        IEnumerable<IsRoutable> GetSimilarSlugs(string contentType, string slug);

        /// <summary>
        /// Validates the given slug
        /// </summary>
        bool IsSlugValid(string slug);

        /// <summary>
        /// Defines the slug of a RoutableAspect and validate its unicity
        /// </summary>
        /// <returns>True if the slug has been created, False if a conflict occured</returns>
        bool ProcessSlug(IsRoutable part);

    }
}