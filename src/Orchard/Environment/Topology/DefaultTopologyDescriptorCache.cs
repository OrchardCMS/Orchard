using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology.Models;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.Topology {
    public class DefaultTopologyDescriptorCache : ITopologyDescriptorCache {
        readonly IDictionary<string, ShellTopologyDescriptor> _cache= new Dictionary<string, ShellTopologyDescriptor>();
        private readonly IAppDataFolder _appDataFolder;
        private const string TopologyCacheFileName = "cache.dat";

        public DefaultTopologyDescriptorCache(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

        }

        public ILogger Logger { get; set; }
        private Localizer T { get; set; }

        #region Implementation of ITopologyDescriptorCache

        public ShellTopologyDescriptor Fetch(string name) {
            VerifyCacheFile();
            ShellTopologyDescriptor value;
            return _cache.TryGetValue(name, out value) ? value : null;
        }

        public void Store(string name, ShellTopologyDescriptor descriptor) {
            VerifyCacheFile();
            _cache[name] = descriptor;
        }

        #endregion

        private void VerifyCacheFile() {
            if (!_appDataFolder.FileExists(TopologyCacheFileName)) {
                _appDataFolder.CreateFile(TopologyCacheFileName, String.Empty);
            }
        }
    }
}
