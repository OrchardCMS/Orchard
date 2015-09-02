using System;

namespace Orchard.Tasks.Locking.Services {
    /// <summary>
    /// Provides distributed locking functionality.
    /// </summary>
    public interface IDistributedLockService : ISingletonDependency {
        /// <summary>
        /// Tries to acquire a lock on the specified name for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out. A null value will cause the method to return immedieately if no lock could be acquired.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        bool TryAcquireLockForMachine(string name, TimeSpan? maxValidFor, TimeSpan? timeout, out DistributedLock @lock);

        /// <summary>
        /// Acquires a lock with the specified parameters for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out. A null value will cause the method to block indefinitely until a lock can be acquired.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        DistributedLock AcquireLockForMachine(string name, TimeSpan? maxValidFor, TimeSpan? timeout);

        /// <summary>
        /// Tries to acquire a lock on the specified name for the current thread.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out. A null value will cause the method to return immedieately if no lock could be acquired.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        bool TryAcquireLockForThread(string name, TimeSpan? maxValidFor, TimeSpan? timeout, out DistributedLock @lock);

        /// <summary>
        /// Acquires a lock with the specified parameters for the current thread.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired before timing out. A null value will cause the method to block indefinitely until a lock can be acquired.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        DistributedLock AcquireLockForThread(string name, TimeSpan? maxValidFor, TimeSpan? timeout);

        /// <summary>
        /// Disposes the specified lock.
        /// </summary>
        void ReleaseLock(DistributedLock @lock);
    }

    public static class DistributedLockServiceExtensions {
        /// <summary>
        /// Tries to acquire a lock on the specified name for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        public static bool TryAcquireLockForMachine(this IDistributedLockService service, string name, TimeSpan? maxValidFor, out DistributedLock @lock) {
            return service.TryAcquireLockForMachine(name, maxValidFor, null, out @lock);
        }

        /// <summary>
        /// Tries to acquire a lock on the specified name for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        public static bool TryAcquireLockForMachine(this IDistributedLockService service, string name, out DistributedLock @lock) {
            return service.TryAcquireLockForMachine(name, null, null, out @lock);
        }

        /// <summary>
        /// Acquires a lock with the specified parameters for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        public static DistributedLock AcquireLockForMachine(this IDistributedLockService service, string name, TimeSpan? maxValidFor) {
            return service.AcquireLockForMachine(name, maxValidFor, null);
        }

        /// <summary>
        /// Acquires a lock with the specified parameters for the current machine.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        public static DistributedLock AcquireLockForMachine(this IDistributedLockService service, string name) {
            return service.AcquireLockForMachine(name, null, null);
        }

        /// <summary>
        /// Tries to acquire a lock on the specified name for the current thread.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock is allowed. This is a safety net in case the caller fails to release the lock. If null is specified, the lock never expires until it's released by the owner.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        public static bool TryAcquireLockForThread(this IDistributedLockService service, string name, TimeSpan? maxValidFor, out DistributedLock @lock) {
            return service.TryAcquireLockForThread(name, maxValidFor, null, out @lock);
        }

        /// <summary>
        /// Tries to acquire a lock on the specified name for the current thread.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <param name="lock">The acquired lock.</param>
        /// <returns>Returns true if a lock was successfully acquired, false otherwise.</returns>
        public static bool TryAcquireLockForThread(this IDistributedLockService service, string name, out DistributedLock @lock) {
            return service.TryAcquireLockForThread(name, null, null, out @lock);
        }

        /// <summary>
        /// Acquires a lock with the specified parameters for the current thread.
        /// </summary>
        /// <param name="name">The name to use for the lock.</param>
        /// <returns>Returns a lock.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if no lock could be acquired in time.</exception>
        public static DistributedLock AcquireLockForThread(this IDistributedLockService service, string name) {
            return service.AcquireLockForThread(name, null, null);
        }
    }
}