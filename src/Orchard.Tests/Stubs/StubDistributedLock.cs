using System;
using Orchard.Tasks.Locking.Services;

namespace Orchard.Tests.Stubs {
    public class StubDistributedLock : IDistributedLock {
        public bool IsDisposed { get; private set; }
        
        public void Dispose() {
            IsDisposed = true;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
