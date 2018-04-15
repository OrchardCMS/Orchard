using System.IO;
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

        private string _content;
        private string _metadata;

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

            _metadata = FileSystemOutputCacheStorageProvider.GetMetadataPath(appDataFolder, _shellSettings.Name);
            _content = FileSystemOutputCacheStorageProvider.GetContentPath(appDataFolder, _shellSettings.Name);
        }

        public void Sweep() {
            foreach(var filename in _appDataFolder.ListFiles(_metadata).ToArray()) {
                var hash = Path.GetFileName(filename);

                var validUntilUtc = _cacheManager.Get(hash, context => {
                    _signals.When(hash);

                    using (var stream = _appDataFolder.OpenFile(filename)) {
                        var cacheItem = FileSystemOutputCacheStorageProvider.DeserializeMetadata(stream);
                        return cacheItem.ValidUntilUtc;
                    }
                });

                if (_clock.UtcNow > validUntilUtc) {
                    _appDataFolder.DeleteFile(_appDataFolder.Combine(_metadata, hash));
                    _appDataFolder.DeleteFile(_appDataFolder.Combine(_content, hash));
                    _signals.Trigger(filename);
                }
            }
        }
    }
}