namespace Orchard.Warmup.Services {
    public interface IWarmupScheduler : IDependency {
        void Schedule(bool force);
    }
}