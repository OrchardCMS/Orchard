using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.OutputCache.Models
{
    public class CacheSettings : ContentPart
    {
        public const string CacheKey = "Orchard_OutputCache_CacheSettings";

        public CacheSettings(CacheSettingsPart part)
        {
            DefaultCacheDuration = part.DefaultCacheDuration;
            DefaultCacheGraceTime = part.DefaultCacheGraceTime;
            DefaultMaxAge = part.DefaultMaxAge;
            VaryByQueryStringIsExclusive = part.VaryByQueryStringIsExclusive;
            VaryByQueryStringParameters = string.IsNullOrWhiteSpace(part.VaryByQueryStringParameters) ? null : part.VaryByQueryStringParameters.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            VaryByRequestHeaders = string.IsNullOrWhiteSpace(part.VaryByRequestHeaders) ? new HashSet<string>() : new HashSet<string>(part.VaryByRequestHeaders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
            VaryByRequestHeaders.Add("HOST"); // Always vary by host name/tenant.
            VaryByRequestCookies = string.IsNullOrWhiteSpace(part.VaryByRequestCookies) ? new HashSet<string>() : new HashSet<string>(part.VaryByRequestCookies.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
            IgnoredUrls = string.IsNullOrWhiteSpace(part.IgnoredUrls) ? null : part.IgnoredUrls.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            IgnoreNoCache = part.IgnoreNoCache;
            VaryByCulture = part.VaryByCulture;
            CacheAuthenticatedRequests = part.CacheAuthenticatedRequests;
            VaryByAuthenticationState = part.VaryByAuthenticationState;
            DebugMode = part.DebugMode;
        }

        public int DefaultCacheDuration { get; private set; }
        public int DefaultCacheGraceTime { get; private set; }
        public int DefaultMaxAge { get; private set; }
        public bool VaryByQueryStringIsExclusive { get; private set; }
        public IEnumerable<string> VaryByQueryStringParameters { get; private set; }
        public ISet<string> VaryByRequestHeaders { get; private set; }
        public ISet<string> VaryByRequestCookies { get; private set; }
        public IEnumerable<string> IgnoredUrls { get; private set; }
        public bool IgnoreNoCache { get; private set; }
        public bool VaryByCulture { get; private set; }
        public bool CacheAuthenticatedRequests { get; private set; }
        public bool VaryByAuthenticationState { get; private set; }
        public bool DebugMode { get; private set; }
    }
}