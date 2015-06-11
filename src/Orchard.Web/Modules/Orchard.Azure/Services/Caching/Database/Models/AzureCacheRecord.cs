using Orchard.Environment.Extensions;

namespace Orchard.Azure.Services.Caching.Database.Models {

    /// <summary>
    /// Fake record in order to force the mappings to be updated
    /// whenever the feature is enabled/disabled.
    /// </summary>
    [OrchardFeature(Constants.DatabaseCacheFeatureName)]
    public class AzureCacheRecord {
        public virtual int Id { get; set; }
    }
}