using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Implementation.Storage;

namespace Orchard.Alias.Implementation.Updater {
    public interface IAliasHolderUpdater : IDependency {
        void Refresh();
    }

    public class AliasHolderUpdater : IAliasHolderUpdater {
        private readonly IAliasHolder _aliasHolder;
        private readonly IAliasStorage _storage;
        private readonly IAliasUpdateCursor _cursor;

        public AliasHolderUpdater(IAliasHolder aliasHolder, IAliasStorage storage, IAliasUpdateCursor cursor) {
            _aliasHolder = aliasHolder;
            _storage = storage;
            _cursor = cursor;
        }

        public void Refresh() {
            // only retreive aliases which have not been processed yet
            var aliases = _storage.List(x => x.Id > _cursor.Cursor).ToArray();

            // update the last processed id
            if (aliases.Any()) {
                _cursor.Cursor = aliases.Last().Item5;
                _aliasHolder.SetAliases(aliases.Select(alias => new AliasInfo { Path = alias.Item1, Area = alias.Item2, RouteValues = alias.Item3 }));
            }
        }
    }
}