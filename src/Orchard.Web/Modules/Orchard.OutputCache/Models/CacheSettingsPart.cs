using Orchard.ContentManagement;

namespace Orchard.OutputCache.Models {
    public class CacheSettingsPart : ContentPart<CacheSettingsPartRecord> {
        public const string CacheKey = "CacheSettingsPart";

        public int DefaultCacheDuration {
            get { return Record.DefaultCacheDuration; }
            set { Record.DefaultCacheDuration = value; }
        }

        public int DefaultMaxAge {
            get { return Record.DefaultMaxAge; }
            set { Record.DefaultMaxAge = value; }
        }

        public string VaryQueryStringParameters {
            get { return Record.VaryQueryStringParameters; }
            set { Record.VaryQueryStringParameters = value; }
        }

        public string IgnoredUrls {
            get { return Record.IgnoredUrls; }
            set { Record.IgnoredUrls = value; }
        }

        public bool ApplyCulture {
            get { return Record.ApplyCulture; }
            set { Record.ApplyCulture = value; }
        }

        public bool DebugMode {
            get { return Record.DebugMode; }
            set { Record.DebugMode = value; }
        }
    }
}