using System.Collections.Generic;
using Orchard.Collections;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Services {
    public interface IRecycleBin : IDependency {
        /// <summary>
        /// Returns all removed content items.
        /// </summary>
        IPageOfItems<ContentItem> List(int page, int pageSize);

        /// <summary>
        /// Returns all removed content items.
        /// </summary>
        IPageOfItems<T> List<T>(int page, int pageSize) where T : class, IContent;

        /// <summary>
        /// Returns the specified list of content items from the recycle bin.
        /// </summary>
        IEnumerable<ContentItem> GetMany(IEnumerable<int> contentItemIds, QueryHints hints = null);

        /// <summary>
        /// Returns the specified list of content items from the recycle bin.
        /// </summary>
        IEnumerable<T> GetMany<T>(IEnumerable<int> contentItemIds, QueryHints hints = null) where T : class, IContent;

        /// <summary>
        /// Restores the specified content item.
        /// </summary>
        ContentItem Restore(ContentItem contentItem);
    }
}