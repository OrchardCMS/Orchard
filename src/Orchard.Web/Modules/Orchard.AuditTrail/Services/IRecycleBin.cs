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
        /// Restores the specified content item.
        /// </summary>
        ContentItem Restore(ContentItem contentItem);
    }
}