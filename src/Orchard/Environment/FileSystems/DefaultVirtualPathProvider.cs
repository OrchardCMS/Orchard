using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using Orchard.Caching.Providers;
using Orchard.Services;

namespace Orchard.Environment.FileSystems {
    public class DefaultVirtualPathProvider : IVirtualPathProvider {
        private readonly IClock _clock;
        private readonly IList<IVolatileSink> _sinks = new List<IVolatileSink>();
        //private string _cachePrefix = Guid.NewGuid().ToString("n");

        public DefaultVirtualPathProvider(IClock clock) {
            _clock = clock;
        }

        public string ReadAllText(string virtualPath) {
            using (var stream = VirtualPathProvider.OpenFile(virtualPath)) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
            //var cd = HostingEnvironment.VirtualPathProvider.. .GetCacheDependency(virtualPath, null, _clock.UtcNow);
            //HostingEnvironment.Cache.Add(
            //    _cachePrefix + virtualPath,
            //    virtualPath,
            //    cd,
            //    NoAbsoluteExpiration,
            //    );
        }

        public IVolatileSignal WhenPathChanges(string virtualPath) {

            return new VirtualPathSignal(this, virtualPath);
        }


        class VirtualPathSignal : IVolatileSignal {
            private readonly string _virtualPath;

            public VirtualPathSignal(DefaultVirtualPathProvider provider, string virtualPath) {
                _virtualPath = virtualPath;
                Provider = provider;
            }

            public IVolatileProvider Provider { get; set; }
        }

        public void Enlist(IVolatileSink sink) {
            _sinks.Add(sink);
        }
    }
}