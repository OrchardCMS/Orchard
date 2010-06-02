using System;
using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Tasks.Indexing {
    public interface IIndexingTaskManager : IDependency {
        void CreateTask(ContentItem contentItem);
        IEnumerable<IIndexingTask> GetTasks(DateTime? createdAfter);
        void DeleteTasks(DateTime? createdBefore);
        void DeleteTasks(ContentItem contentItem);
    }
}