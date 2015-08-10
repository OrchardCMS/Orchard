using System;

namespace Orchard.Tasks.Locking {
    /// <summary>
    /// Provides distributed locking functionality.
    /// </summary>
    public interface ILockService : IDependency {
        /// <summary>
        /// Tries to acquire a lock on the specified name.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxLifetime">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out</param>
        /// <returns>Returns a lock if one was successfully acquired, null otherwise.</returns>
        ILock TryAcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout);

        /// <summary>
        /// Acquires a lock with the specified parameters.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxLifetime">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out</param>
        /// <returns>Returns a lock if one was successfully acquired.</returns>
        /// <exception cref="TimeoutException">Thrown if the lock couldn't be acquired.</exception>
        ILock AcquireLock(string name, TimeSpan maxLifetime, TimeSpan timeout);
    }
}