using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Orchard.Tasks {

    public interface IBackgroundService : IDependency {
        void Sweep();
        void Terminate();
    }

    [UsedImplicitly]
    public class BackgroundService : IBackgroundService {
        private readonly IEnumerable<IBackgroundTask> _tasks;
        private readonly ITransactionManager _transactionManager;
        private readonly string _shellName;
        private bool _shuttingDown;

        private AutoResetEvent _finishedEvent = new AutoResetEvent(true);

        public BackgroundService(
            IEnumerable<IBackgroundTask> tasks, 
            ITransactionManager transactionManager, 
            ShellSettings shellSettings) {

            _tasks = tasks;
            _transactionManager = transactionManager;
            _shellName = shellSettings.Name;
            Logger = NullLogger.Instance;
            IsolationLevel = IsolationLevel.ReadCommitted;
        }

        public ILogger Logger { get; set; }
        public IsolationLevel IsolationLevel { get; set; }

        public void Sweep() {
            foreach(var task in _tasks) {
                if (_shuttingDown) {
                    return;
                }

                try {
                    _finishedEvent.Reset();
                    _transactionManager.RequireNew(IsolationLevel);
                    task.Sweep();
                }
                catch (Exception e) {
                    _transactionManager.Cancel();
                    Logger.Error(e, "Error while processing background task on tenant '{0}'.", _shellName);
                }
                finally
                {
                    _finishedEvent.Set();
                }
            }
        }

        public void Terminate() {
            Logger.Debug("Background service terminating...");
            _shuttingDown = true;

            foreach (var task in _tasks.Where(t => t is ITerminatable)) {
                try {
                    ((ITerminatable)task).Terminate();
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while terminating background task {0}", task.GetType().Name);
                }
            }

            if (_finishedEvent != null) {
                _finishedEvent.WaitOne(TimeSpan.FromSeconds(90));
                _finishedEvent.Dispose();
                _finishedEvent = null;
            }
        }
    }
}
