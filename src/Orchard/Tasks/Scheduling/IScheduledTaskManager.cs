using System;
using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Tasks.Scheduling {
    public interface IScheduledTaskManager : IDependency {
        void CreateTask(string taskType, DateTime scheduledUtc, ContentItem contentItem);
        
        IEnumerable<IScheduledTask> GetTasks(ContentItem contentItem);
        IEnumerable<IScheduledTask> GetTasks(string taskType, DateTime? scheduledBeforeUtc = null);

        void DeleteTasks(ContentItem contentItem, Func<IScheduledTask, bool> predicate = null);
    }
}