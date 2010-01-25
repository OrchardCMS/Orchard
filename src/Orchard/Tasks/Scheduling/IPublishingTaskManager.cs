using System;
using Orchard.ContentManagement;

namespace Orchard.Tasks.Scheduling {
    public interface IPublishingTaskManager : IDependency {
        IScheduledTask GetPublishTask(ContentItem item);
        void Publish(ContentItem item, DateTime scheduledUtc);
        void DeleteTasks(ContentItem item);
    }
}