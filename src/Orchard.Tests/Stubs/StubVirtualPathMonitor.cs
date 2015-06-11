using Orchard.Caching;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Tests.Stubs {
    public class StubVirtualPathMonitor : IVirtualPathMonitor {
        public class Token : IVolatileToken {
            public bool IsCurrent { get; set; }
        }
        public IVolatileToken WhenPathChanges(string virtualPath) {
            return new Token();
        }
    }
}
