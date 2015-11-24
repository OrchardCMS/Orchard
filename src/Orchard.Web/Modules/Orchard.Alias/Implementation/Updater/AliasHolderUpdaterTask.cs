using System;
using Orchard.Environment;
using Orchard.Tasks;
using Orchard.Logging;

namespace Orchard.Alias.Implementation.Updater {
    public class AliasHolderUpdaterTask : IOrchardShellEvents, IBackgroundTask {
        private readonly IAliasHolderUpdater _aliasHolderUpdater;

        public ILogger Logger { get; set; }

        public AliasHolderUpdaterTask(IAliasHolderUpdater aliasHolderUpdater) {
            _aliasHolderUpdater = aliasHolderUpdater;
            Logger = NullLogger.Instance;
        }

        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        private void Refresh() {
            try {
                _aliasHolderUpdater.Refresh();
            }
            catch (Exception ex) {
                Logger.Error(ex, "Exception during Alias refresh");
            }
        }

        public void Sweep() {
            Refresh();
        }
    }
}
