using Orchard.Logging;

namespace Orchard.Tasks {

    public interface IBackgroundService : IDependency {
        void Sweep();
    }

    public class BackgroundService : IBackgroundService {
        private readonly IBackgroundTask _tasks;

        public BackgroundService(IBackgroundTask tasks) {
            _tasks = tasks;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            _tasks.Sweep();
        }
    }
}
