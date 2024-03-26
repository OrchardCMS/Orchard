using Orchard.Tasks;

namespace Orchard.JobsQueue.Services {
    public class JobsQueueBackgroundTask : Component, IBackgroundTask {
        private readonly IJobsQueueProcessor _jobsQueueProcessor;
        public JobsQueueBackgroundTask(IJobsQueueProcessor jobsQueueProcessor) {
            _jobsQueueProcessor = jobsQueueProcessor;
        }

        public void Sweep() {
            _jobsQueueProcessor.ProcessQueue(1, uint.MaxValue);
        }
    }
}