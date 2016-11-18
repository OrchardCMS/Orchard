using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public interface ITaxonomyExtensionsService : IDependency {

        /// <summary>
        /// Returns all the <see cref="ContentTypeDefinition" /> data for content types containing a Term Part.
        /// </summary>
        /// <returns>A list of distinct <see cref="ContentTypeDefinition"/> of terms.</returns>
        IEnumerable<ContentTypeDefinition> GetAllTermTypes();

        /// <summary>
        /// Creates a new Content Type for the terms of a <see cref="TaxonomyPart"/> and attaches a LocalizationPart to it.
        /// </summary>
        /// <param name="taxonomy">The taxonomy to create a term content type for.</param>
        void CreateLocalizedTermContentType(TaxonomyPart taxonomy);
    }
}