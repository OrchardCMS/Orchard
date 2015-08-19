using System;

namespace Orchard.Tasks.Locking.Services {
    /// <summary>
    /// Represents a distributed lock.
    /// </summary>
    public interface IDistributedLock : IDisposable {
        int Id { get; }
        string Name { get; }
    }
}