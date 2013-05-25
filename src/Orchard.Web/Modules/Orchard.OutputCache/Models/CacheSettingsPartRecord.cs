using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.OutputCache.Models {
    public class CacheSettingsPartRecord : ContentPartRecord {
        public virtual int DefaultCacheDuration { get; set; }
        public virtual int DefaultMaxAge { get; set; }
        public virtual bool DebugMode { get; set; }
        public virtual bool ApplyCulture { get; set; }

        [StringLengthMax]
        public virtual string VaryQueryStringParameters { get; set; }
        
        [StringLengthMax]
        public virtual string IgnoredUrls { get; set; }
    }
}