using System.Collections.Generic;
using Orchard.Logging;

namespace Orchard.Tasks {

    public interface IBackgroundService : IDependency {
        void Sweep();
    }

    public class BackgroundService : IBackgroundService {
        private readonly IEnumerable<IBackgroundTask> _tasks;

        public BackgroundService(IEnumerable<IBackgroundTask> tasks) {
            _tasks = tasks;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            _tasks.Invoke(task => task.Sweep(), Logger);
        }
    }

}
