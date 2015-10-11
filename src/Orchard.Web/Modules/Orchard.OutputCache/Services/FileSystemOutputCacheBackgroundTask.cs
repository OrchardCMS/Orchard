using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.Services;
using Orchard.Tasks;

namespace Orchard.OutputCache.Services {
    [OrchardFeature("Orchard.OutputCache.FileSystem")]
    /// <summary>
    /// A background task deleting all App_Data output cache content.
    /// </summary>
    public class FileSystemOutputCacheBackgroundTask : IBackgroundTask {
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;

        private string _root;

        public FileSystemOutputCacheBackgroundTask(
            IAppDataFolder appDataFolder, 
            ShellSettings shellSettings,
            ICacheManager cacheManager,
            IClock clock,
            ISignals signals) {
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
            _cacheManager = cacheManager;
            _clock = clock;
            _signals = signals;

            _root = _appDataFolder.Combine("OutputCache", _shellSettings.Name);
        }

        public void Sweep() {
            foreach(var filename in _appDataFolder.ListFiles(_root).ToArray()) {
                var validUntilUtc = _cacheManager.Get(filename, context => {
                    _signals.When(filename);

                    using (var stream = _appDataFolder.OpenFile(filename)) {
                        var cacheItem = FileSystemOutputCacheStorageProvider.Deserialize(stream);
                        return cacheItem.ValidUntilUtc;
                    }
                });

                if (_clock.UtcNow > validUntilUtc) {
                    _appDataFolder.DeleteFile(filename);
                    _signals.Trigger(filename);
                }
            }
        }
    }
}