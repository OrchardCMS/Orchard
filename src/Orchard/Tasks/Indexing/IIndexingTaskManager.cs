using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Loads all indexing tasks created after to a specific date and time
        /// </summary>
        IEnumerable<IIndexingTask> GetTasks(DateTime? createdAfter);
        
        /// <summary>
        /// Deletes all indexing tasks assigned to a specific content item
        /// </summary>
        /// <param name="contentItem"></param>
        void DeleteTasks(ContentItem contentItem);
    }
}