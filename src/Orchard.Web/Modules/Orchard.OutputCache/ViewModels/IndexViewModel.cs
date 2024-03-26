using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.ViewModels {
    public class IndexViewModel {
        public List<CacheRouteConfig> RouteConfigs { get; set; }
        [Range(0, Int32.MaxValue), Required] public int DefaultCacheDuration { get; set; }
        [Range(0, Int32.MaxValue), Required] public int DefaultCacheGraceTime { get; set; }
        [Range(0, Int32.MaxValue), Required] public int DefaultMaxAge { get; set; }
        public string VaryByQueryStringParameters { get; set; }
        public string VaryByRequestHeaders { get; set; }
        public string VaryByRequestCookies { get; set; }
        public string IgnoredUrls { get; set; }
        public bool IgnoreNoCache { get; set; }
        public bool VaryByCulture { get; set; }
        public bool CacheAuthenticatedRequests { get; set; }
        public bool VaryByAuthenticationState { get; set; }
        public bool DebugMode { get; set; }
    }
}