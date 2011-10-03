using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Events;
using Orchard.Logging;

namespace Orchard.Tasks {

    public interface IBackgroundService : IDependency {
        void Sweep();
    }

    [UsedImplicitly]
    public class BackgroundService : IBackgroundService {
        private readonly IEnumerable<IEventHandler> _tasks;

        public BackgroundService(IEnumerable<IEventHandler> tasks) {
            _tasks = tasks;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            foreach(var task in _tasks.OfType<IBackgroundTask>()) {
                using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew)) {
                    try {
                        task.Sweep();
                        scope.Complete();
                    }
                    catch (Exception e) {
                        Logger.Error(e, "Error while processing background task");
                    }
                }
            }
        }
    }
}
