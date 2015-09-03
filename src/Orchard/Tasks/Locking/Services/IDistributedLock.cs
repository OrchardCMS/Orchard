using System;

namespace Orchard.Tasks.Locking.Services {

    /// <summary>
    /// Represents a distributed lock. />
    /// </summary>
    public interface IDistributedLock : IDisposable {
        string Name { get; }
    }
}