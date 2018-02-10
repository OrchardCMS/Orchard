using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.Environment {
    /// <summary>
    /// Provides ability to connect Shims to the OrchardHostContainer
    /// </summary>
    public static class OrchardHostContainerRegistry {
        private static readonly IList<Weak<IShim>> _shims = new List<Weak<IShim>>();
        private static IOrchardHostContainer _hostContainer;
        private static readonly object _syncLock = new object();

        public static void RegisterShim(IShim shim) {
            lock (_syncLock) {
                CleanupShims();

                _shims.Add(new Weak<IShim>(shim));
                shim.HostContainer = _hostContainer;
            }
        }

        public static void RegisterHostContainer(IOrchardHostContainer container) {
            lock (_syncLock) {
                CleanupShims();

                _hostContainer = container;
                RegisterContainerInShims();
            }
        }

        private static void RegisterContainerInShims() {
            foreach (var shim in _shims) {
                var target = shim.Target;
                if (target != null) {
                    target.HostContainer = _hostContainer;
                }
            }
        }

        private static void CleanupShims() {
            for (int i = _shims.Count - 1; i >= 0; i--) {
                if (_shims[i].Target == null)
                    _shims.RemoveAt(i);
            }
        }
    }
}
