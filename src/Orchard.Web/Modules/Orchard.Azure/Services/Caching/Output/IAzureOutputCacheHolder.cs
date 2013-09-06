using System;
using Microsoft.ApplicationServer.Caching;

namespace Orchard.Azure.Services.Caching.Output {
    public interface IAzureOutputCacheHolder : ISingletonDependency {
        DataCache TryGetDataCache(Func<DataCache> builder);
    }

    public class AzureOutputCacheHolder : IAzureOutputCacheHolder {
        private readonly object _synLock = new object();
        private DataCache _dataCache;

        public DataCache TryGetDataCache(Func<DataCache> builder) {
            lock (_synLock) {
                if (_dataCache != null) {
                    return _dataCache;
                }

                return _dataCache = builder();
            }
        }
    }
}