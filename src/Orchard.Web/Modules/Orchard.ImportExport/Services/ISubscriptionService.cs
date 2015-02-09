namespace Orchard.ImportExport.Services {
    public interface ISubscriptionService : IDependency {
        string GetDeploymentFile(int subscriptionId, string executionId, bool exportIfNotFound = true);
        void ScheduleSubscriptionTask(int subscriptionId);
        string RunSubscriptionTask(int subscriptionId);
    }
}
