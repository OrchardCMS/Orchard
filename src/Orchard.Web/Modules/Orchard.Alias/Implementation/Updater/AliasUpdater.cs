using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Implementation.Storage;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Tasks;
using Orchard.Logging;

namespace Orchard.Alias.Implementation.Updater {
    public class AliasHolderUpdater : IOrchardShellEvents, IBackgroundTask {
        private readonly IAliasHolder _aliasHolder;
        private readonly IAliasStorage _storage;

        public ILogger Logger { get; set; }

        public AliasHolderUpdater(IAliasHolder aliasHolder, IAliasStorage storage) {
            _aliasHolder = aliasHolder;
            _storage = storage;
            Logger = NullLogger.Instance;
        }

        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        private void Refresh() {
            try {
                var aliases = _storage.List();
                _aliasHolder.SetAliases(aliases.Select(alias => new AliasInfo { Path = alias.Item1, Area = alias.Item2, RouteValues = alias.Item3 }));
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
