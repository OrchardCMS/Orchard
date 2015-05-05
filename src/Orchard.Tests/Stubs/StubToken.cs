using Orchard.Caching;

namespace Orchard.Tests.Stubs {
    public class StubToken : IVolatileToken {
        public bool IsCurrent {
            get { return true; }
        }
    }
}
