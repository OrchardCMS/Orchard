using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.OutputCache.ViewModels {
    public class IndexViewModel {
        public List<RouteConfiguration> RouteConfigurations { get; set; }
        [Range(0, int.MaxValue), Required]
        public int DefaultCacheDuration { get; set; }
        [Range(0, int.MaxValue), Required]
        public int DefaultMaxAge { get; set; }
        public string IgnoredUrls { get; set; }
        public bool ApplyCulture { get; set; }
        public bool DebugMode { get; set; }
        public string VaryQueryStringParameters { get; set; }
        public string VaryRequestHeaders { get; set; }
    }
}