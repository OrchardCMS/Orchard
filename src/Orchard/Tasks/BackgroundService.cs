using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.ContentManagement;

namespace Orchard.Tasks {

    public interface IBackgroundService : IDependency {
        void Sweep();
    }

    [UsedImplicitly]
    public class BackgroundService : IBackgroundService {
        private readonly IEnumerable<IBackgroundTask> _tasks;
        private readonly ITransactionManager _transactionManager;
        private readonly string _shellName;
        private readonly IContentManager _contentManager;

        public BackgroundService(
            IEnumerable<IBackgroundTask> tasks, 
            ITransactionManager transactionManager, 
            ShellSettings shellSettings,
            IContentManager contentManager) {
            _tasks = tasks;
            _transactionManager = transactionManager;
            _shellName = shellSettings.Name;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            foreach(var task in _tasks) {
                try {
                    _transactionManager.RequireNew();
                    task.Sweep();
                }
                catch (Exception e) {
                    _transactionManager.Cancel();
                    Logger.Error(e, "Error while processing background task on tenant '{0}'.", _shellName);
                }
            }
        }
    }
}
