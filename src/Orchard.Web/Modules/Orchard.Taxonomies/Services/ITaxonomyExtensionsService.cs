using System.Collections.Generic;
using Orchard.ContentManagement;
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

        /// <summary>
        /// Returns the parent Taxonomy of the specified ContentItem, if it exists
        /// </summary>
        /// <param name="part">The <see cref="TermPart"/> for which to search the parent Taxonomy.</param>
        /// <returns>The parent Taxonomy as a <see cref="ContentItem"/> if it exists, otherwise null.</returns>
        ContentItem GetParentTaxonomy(TermPart part);

        /// <summary>
        /// Returns the parent Term of the specified ContentItem, if it exists
        /// </summary>
        /// <param name="part">The <see cref="TermPart"/> for which to search the parent Term.</param>
        /// <returns>The parent Term as a <see cref="ContentItem"/> if it exists, otherwise null.</returns>
        ContentItem GetParentTerm(TermPart part);

        /// <summary>
        /// Returns the master item of the specified Content Item, if it exists.
        /// </summary>
        /// <param name="item">The item for which to search the master.</param>
        /// <returns>The master item if it exists, otherwise null.</returns>
        IContent GetMasterItem(IContent item);

        /// <summary>
        /// Regenerates the autoroute for the specified <see cref="ContentItem"/>
        /// </summary>
        /// <param name="item">The item for which to regenerate the autoroute.</param>
        void RegenerateAutoroute(ContentItem item);


    }
}