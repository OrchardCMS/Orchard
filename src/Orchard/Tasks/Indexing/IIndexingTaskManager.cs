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
        /// Returns the Date Time of the last task created
        /// </summary>
        DateTime GetLastTaskDateTime();

        /// <summary>
        /// Deletes all indexing tasks assigned to a specific content item
        /// </summary>
        /// <param name="contentItem"></param>
        void DeleteTasks(ContentItem contentItem);
    }
}