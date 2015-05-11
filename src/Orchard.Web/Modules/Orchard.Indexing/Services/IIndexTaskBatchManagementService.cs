using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Indexing.Services {
    /// <summary>
    /// Manages the batches for indexing tasks.
    /// </summary>
    public interface IIndexTaskBatchManagementService : IDependency {

        /// <summary>
        /// Registers a content type for the <see cref="CreateUpdateIndexTaskBackgroundTask"/>.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        void RegisterContentType(string contentType);
        
        /// <summary>
        /// Returns a list of a list of the registered content items.
        /// </summary>
        IEnumerable<IEnumerable<ContentItem>> GetNextBatchOfContentItemsToIndex();
    }
}