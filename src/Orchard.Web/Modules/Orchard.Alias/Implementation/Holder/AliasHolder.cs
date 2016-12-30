using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Orchard.Alias.Implementation.Map;
using Orchard.Alias.Implementation.Updater;

namespace Orchard.Alias.Implementation.Holder {
    public class AliasHolder : IAliasHolder {
        private readonly Lazy<IAliasHolderUpdater> _aliasHolderUpdater;
        private readonly ConcurrentDictionary<string, AliasMap> _aliasMaps;

        public AliasHolder(Lazy<IAliasHolderUpdater> aliasHolderUpdater) {
            _aliasHolderUpdater = aliasHolderUpdater;
            _aliasMaps = new ConcurrentDictionary<string, AliasMap>(StringComparer.OrdinalIgnoreCase);
        }

        private ConcurrentDictionary<string, AliasMap> GetOrRefreshAliasMaps() {
            lock (_aliasMaps) {
                if (_aliasMaps.Count == 0)
                    _aliasHolderUpdater.Value.Refresh();
            }

            return _aliasMaps;
        }

        public void SetAliases(IEnumerable<AliasInfo> aliases) {
            var grouped = aliases.GroupBy(alias => alias.Area ?? String.Empty, StringComparer.InvariantCultureIgnoreCase);

            foreach (var group in grouped) {
                var map = GetMap(group.Key);

                foreach (var alias in group) {
                    map.Insert(alias);
                }
            }
        }

        public void SetAlias(AliasInfo alias) {
            foreach (var map in GetOrRefreshAliasMaps().Values) {
                map.Remove(alias);
            }

            GetMap(alias.Area).Insert(alias);
        }

        public IEnumerable<AliasMap> GetMaps() {
            return GetOrRefreshAliasMaps().Values;
        }

        public AliasMap GetMap(string areaName) {
            return GetOrRefreshAliasMaps().GetOrAdd(areaName ?? String.Empty, key => new AliasMap(key));
        }

        public void RemoveAlias(AliasInfo aliasInfo) {
            GetMap(aliasInfo.Area ?? String.Empty).Remove(aliasInfo);
        }
    }
}