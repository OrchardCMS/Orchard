using System;
using Orchard.ContentManagement;

namespace Orchard.Tasks.Indexing {
    public interface IIndexingTaskManager : IDependency {
        /// <summary>
        /// Adds a new entry in the index task table in order to create an index for the specified content item.
        /// </summary>
        void CreateUpdateIndexTask(ContentItem contentItem);

        /// <summary>
        /// Adds a new entry in the index task table in order to delete an existing index for the specified content item.
        /// </summary>
        void CreateDeleteIndexTask(ContentItem contentItem);
    }
}