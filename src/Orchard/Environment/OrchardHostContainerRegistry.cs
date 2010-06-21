using System.Collections.Generic;

namespace Orchard.Environment {
    /// <summary>
    /// Provides ability to connect Shims and the OrchardHostContainer
    /// </summary>
    public static class OrchardHostContainerRegistry {
        private static readonly IList<IShim> _shims = new List<IShim>();
        private static IOrchardHostContainer _hostContainer;

        public static void RegisterShim(IShim shim) {
            _shims.Add(shim);
            shim.HostContainer = _hostContainer;
        }

        public static void RegisterHostContainer(IOrchardHostContainer container) {
            if (object.ReferenceEquals(_hostContainer, container))
                return;

            UnregisterContainerShims();
            _hostContainer = container;
            RegisterContainerInShims();
        }

        private static void UnregisterContainerShims() {
            foreach (var shim in _shims) {
                shim.HostContainer = null;
            }
        }

        private static void RegisterContainerInShims() {
            foreach (var shim in _shims) {
                shim.HostContainer = _hostContainer;
            }
        }
    }
}
