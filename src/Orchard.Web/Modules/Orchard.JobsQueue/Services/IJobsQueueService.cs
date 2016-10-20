using Orchard.JobsQueue.Models;

namespace Orchard.JobsQueue.Services {
    public interface IJobsQueueService : IDependency {
        QueuedJobRecord Enqueue(string message, object parameters, int priority);
    }
}
