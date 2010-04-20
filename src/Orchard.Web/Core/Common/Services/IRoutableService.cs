using System;
using System.Collections.Generic;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Services {
    public interface IRoutableService : IDependency {
        void FillSlug<TModel>(TModel model) where TModel : RoutableAspect;
        void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : RoutableAspect;
        string GenerateUniqueSlug(string slugCandidate, IEnumerable<string> existingSlugs);

        /// <summary>
        /// Returns any content item of the specified content type with similar slugs
        /// </summary>
        string[] GetSimilarSlugs(string contentType, string slug);

        /// <summary>
        /// Validates the given slug
        /// </summary>
        bool IsSlugValid(string slug);

        /// <summary>
        /// Defines the slug of a RoutableAspect and validate its unicity
        /// </summary>
        void ProcessSlug(RoutableAspect part);

    }
}