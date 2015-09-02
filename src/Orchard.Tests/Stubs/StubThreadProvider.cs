using Orchard.Environment;

namespace Orchard.Tests.Stubs {
    public class StubThreadProvider : IThreadProvider {
        public StubThreadProvider() {
            ManagedThreadId = 1;
        }

        public int ManagedThreadId { get; set; }

        public int GetCurrentThreadId() {
            return ManagedThreadId;
        }
    }
}
