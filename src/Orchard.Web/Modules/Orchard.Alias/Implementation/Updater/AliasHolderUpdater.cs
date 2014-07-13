using System;
using System.Linq;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Implementation.Storage;
using Orchard.Environment;
using Orchard.Tasks;
using Orchard.Logging;
using Orchard.Environment.Extensions;

namespace Orchard.Alias.Implementation.Updater {
    public interface IAliasHolderUpdater : IDependency {
        void Refresh();
    }

    public class AliasHolderUpdater : IAliasHolderUpdater {
        private readonly IAliasHolder _aliasHolder;
        private readonly IAliasStorage _storage;
        private readonly IAliasUpdateCursor _cursor;

        public ILogger Logger { get; set; }

        public AliasHolderUpdater(IAliasHolder aliasHolder, IAliasStorage storage, IAliasUpdateCursor cursor) {
            _aliasHolder = aliasHolder;
            _storage = storage;
            _cursor = cursor;
            Logger = NullLogger.Instance;
        }

        public void Refresh() {
            try {
                // only retreive aliases which have not been processed yet
                var aliases = _storage.List(x => x.Id > _cursor.Cursor).ToArray();

                // update the last processed id
                if (aliases.Any()) {
                    _cursor.Cursor = aliases.Last().Item5;
                    _aliasHolder.SetAliases(aliases.Select(alias => new AliasInfo { Path = alias.Item1, Area = alias.Item2, RouteValues = alias.Item3 }));
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "Exception during Alias refresh");
            }
        }
    }

    public class AliasUpdaterEvent : IOrchardShellEvents {
        
        private readonly IAliasHolderUpdater _aliasHolderUpdater;

        public AliasUpdaterEvent(IAliasHolderUpdater aliasHolderUpdater) {
            _aliasHolderUpdater = aliasHolderUpdater;
        }
        
        void IOrchardShellEvents.Activated() {
            _aliasHolderUpdater.Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }
    }
}
