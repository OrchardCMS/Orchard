using System;
using Orchard.ContentManagement;

namespace Orchard.OutputCache.Models {
    public class CacheSettingsPart : ContentPart {
        public int DefaultCacheDuration {
            get { return this.Retrieve(x => x.DefaultCacheDuration, 300); }
            set { this.Store(x => x.DefaultCacheDuration, value); }
        }

        public int DefaultCacheGraceTime {
            get { return this.Retrieve(x => x.DefaultCacheGraceTime, 60); }
            set { this.Store(x => x.DefaultCacheGraceTime, value); }
        }

        public int DefaultMaxAge {
            get { return this.Retrieve(x => x.DefaultMaxAge); }
            set { this.Store(x => x.DefaultMaxAge, value); }
        }

        public string VaryByQueryStringParameters {
            get {
                return this.Retrieve(
                    x => x.VaryByQueryStringParameters,
                    this.Retrieve<string>("VaryQueryStringParameters") // Migrate from old property name.
                );
            }
            set {
                this.Store(x => x.VaryByQueryStringParameters, value);
                this.Store<string>("VaryQueryStringParameters", null); // Get rid of old property name.
            }
        }

        public string VaryByRequestHeaders {
            get {
                return this.Retrieve(
                    x => x.VaryByRequestHeaders,
                    this.Retrieve<string>("VaryRequestHeaders") // Migrate from old property name.
                );
            }
            set {
                this.Store(x => x.VaryByRequestHeaders, value);
                this.Store<string>("VaryRequestHeaders", null); // Get rid of old property name.
            }
        }

        public string VaryByRequestCookies {
            get { return this.Retrieve(x => x.VaryByRequestCookies); }
            set { this.Store(x => x.VaryByRequestCookies, value); }
        }

        public string IgnoredUrls {
            get { return this.Retrieve(x => x.IgnoredUrls); }
            set { this.Store(x => x.IgnoredUrls, value); }
        }

        public bool IgnoreNoCache {
            get { return this.Retrieve(x => x.IgnoreNoCache); }
            set { this.Store(x => x.IgnoreNoCache, value); }
        }

        public bool VaryByCulture {
            get {
                return this.Retrieve(
                    x => x.VaryByCulture,
                    this.Retrieve<bool>("ApplyCulture") // Migrate from old property name.
                );
            }
            set {
                this.Store(x => x.VaryByCulture, value);
                this.Store<string>("ApplyCulture", null); // Get rid of old property name.
            }
        }

        public bool CacheAuthenticatedRequests {
            get { return this.Retrieve(x => x.CacheAuthenticatedRequests); }
            set { this.Store(x => x.CacheAuthenticatedRequests, value); }
        }

        public bool VaryByAuthenticationState {
            get { return this.Retrieve(x => x.VaryByAuthenticationState); }
            set { this.Store(x => x.VaryByAuthenticationState, value); }
        }

        public bool DebugMode {
            get { return this.Retrieve(x => x.DebugMode); }
            set { this.Store(x => x.DebugMode, value); }
        }
    }
}