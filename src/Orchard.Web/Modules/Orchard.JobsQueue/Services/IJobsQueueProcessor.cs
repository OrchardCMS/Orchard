namespace Orchard.JobsQueue.Services {
    public interface IJobsQueueProcessor : ISingletonDependency {
        void ProcessQueue(int batchSize, uint batchCount);
    }
}