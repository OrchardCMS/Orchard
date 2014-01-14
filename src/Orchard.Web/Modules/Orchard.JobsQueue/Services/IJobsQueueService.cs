using Orchard.Events;
using Orchard.JobsQueue.Models;

namespace Orchard.JobsQueue.Services {
    public interface IJobsQueueService : IEventHandler {
        QueuedJobRecord Enqueue(string message, object parameters, int priority);
    }
}