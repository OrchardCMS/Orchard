namespace Orchard.Tasks.Scheduling {
    public interface IScheduledTaskHandler : IDependency {
        void Process(ScheduledTaskContext context);
    }
}