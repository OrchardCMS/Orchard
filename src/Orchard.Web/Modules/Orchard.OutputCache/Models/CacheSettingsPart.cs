using Orchard.ContentManagement;

namespace Orchard.OutputCache.Models {
    public class CacheSettingsPart : ContentPart {
        public const string CacheKey = "CacheSettingsPart";

        public int DefaultCacheDuration {
            get { return this.Retrieve(x => x.DefaultCacheDuration, 300); }
            set { this.Store(x => x.DefaultCacheDuration, value); }
        }

        public int DefaultMaxAge {
            get { return this.Retrieve(x => x.DefaultMaxAge); }
            set { this.Store(x => x.DefaultMaxAge, value); }
        }

        public string VaryQueryStringParameters {
            get { return this.Retrieve(x => x.VaryQueryStringParameters); }
            set { this.Store(x => x.VaryQueryStringParameters, value); }
        }

        public string VaryRequestHeaders {
            get { return this.Retrieve(x => x.VaryRequestHeaders); }
            set { this.Store(x => x.VaryRequestHeaders, value); }
        }

        public string IgnoredUrls {
            get { return this.Retrieve(x => x.IgnoredUrls); }
            set { this.Store(x => x.IgnoredUrls, value); }
        }

        public bool ApplyCulture {
            get { return this.Retrieve(x => x.ApplyCulture); }
            set { this.Store(x => x.ApplyCulture, value); }
        }

        public bool DebugMode {
            get { return this.Retrieve(x => x.DebugMode); }
            set { this.Store(x => x.DebugMode, value); }
        }
    }
}