using System;
using Orchard.Tasks.Locking.Services;

namespace Orchard.Tests.Stubs {
    public class StubDistributedLock : IDistributedLock {
        public static bool IsAcquired { get; private set; }
        public static string AcquiredByMachineName { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool TryAcquire(string name, string machineName, TimeSpan maxLifetime) {
            if (IsAcquired && machineName != AcquiredByMachineName)
                return false;

            IsAcquired = true;
            AcquiredByMachineName = machineName;
            return true;
        }


        public void Dispose() {
            IsDisposed = true;
            IsAcquired = false;
            AcquiredByMachineName = null;
        }
    }
}
