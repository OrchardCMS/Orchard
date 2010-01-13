namespace Orchard.Core.Scheduling {
    public interface IScheduledTaskHandler : IDependency {
        void Process(ScheduledTaskContext context);
    }
}