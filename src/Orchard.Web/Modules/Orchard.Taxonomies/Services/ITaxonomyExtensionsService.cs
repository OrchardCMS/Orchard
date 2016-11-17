using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Taxonomies.Services {
    public interface ITaxonomyExtensionsService : IDependency {

        /// <summary>
        /// Returns all the <see cref="ContentTypeDefinition" /> data for content types containing a Term Part.
        /// </summary>
        /// <returns>The <see cref="ContentTypeDefinition"/> of terms.</returns>
        IEnumerable<ContentTypeDefinition> GetAllTermTypes();
    }
}