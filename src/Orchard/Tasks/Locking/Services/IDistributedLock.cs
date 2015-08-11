using System;

namespace Orchard.Tasks.Locking.Services {
    /// <summary>
    /// Provides a lock on a provided name.
    /// </summary>
    public interface IDistributedLock : ITransientDependency, IDisposable {
        /// <summary>
        /// Tries to acquire a lock on the specified name.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="machineName">The machine name trying to acquire a lock.</param>
        /// <param name="maxLifetime">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock.</param>
        /// <returns>Returns true if a lock was acquired, false otherwise.</returns>
        bool TryAcquire(string name, string machineName, TimeSpan maxLifetime);
    }
}