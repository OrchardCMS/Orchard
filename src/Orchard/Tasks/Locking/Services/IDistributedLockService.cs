using System;

namespace Orchard.Tasks.Locking.Services {
    /// <summary>
    /// Provides distributed locking functionality.
    /// </summary>
    public interface IDistributedLockService : IDependency {
        /// <summary>
        /// Tries to acquire a distributed lock on a named resource for the current tenant.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If <c>null</c> is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out. A null value will cause the method to block indefinitely until a lock can be acquired.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        bool TryAcquireLock(string name, TimeSpan? maxValidFor, TimeSpan? timeout, out IDistributedLock @lock);

        /// <summary>
        /// Acquires a distributed lock on a named resource for the current tenant.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If <c>null</c> is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out. A null value will cause the method to block indefinitely until a lock can be acquired.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        IDistributedLock AcquireLock(string name, TimeSpan? maxValidFor, TimeSpan? timeout);
    }

    public static class DistributedLockServiceExtensions {
        /// <summary>
        /// Tries to acquire a lock on the specified name for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        public static bool TryAcquireLock(this IDistributedLockService service, string name, TimeSpan? maxValidFor, out IDistributedLock @lock) {
            return service.TryAcquireLock(name, maxValidFor, TimeSpan.Zero, out @lock);
        }

        /// <summary>
        /// Tries to acquire a lock on the specified name for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        public static bool TryAcquireLock(this IDistributedLockService service, string name, out IDistributedLock @lock) {
            return service.TryAcquireLock(name, null, TimeSpan.Zero, out @lock);
        }

        /// <summary>
        /// Acquires a lock with the specified parameters for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        public static IDistributedLock AcquireLock(this IDistributedLockService service, string name, TimeSpan? maxValidFor) {
            return service.AcquireLock(name, maxValidFor, null);
        }

        /// <summary>
        /// Acquires a lock with the specified parameters for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        public static IDistributedLock AcquireLock(this IDistributedLockService service, string name) {
            return service.AcquireLock(name, null, null);
        }
    }
}