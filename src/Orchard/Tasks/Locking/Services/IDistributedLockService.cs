using System;

namespace Orchard.Tasks.Locking.Services {

	/// <summary>
	/// Provides functionality to acquire and release tenant-wide locks which are
	/// distributed across all instances in a web farm.
	/// </summary>
	/// <remarks>
	/// Distributed locks can be used to protect critical sections that should only ever
	/// be executed by a single thread of execution across a whole web farm. The distributed
	/// locks returned by this service are reentrant, i.e. the owner of a lock can reacquire it
	/// multiple times, and must also release it (dispose of it) as many times as it was
	/// acquired for the lock to be released.
	/// </remarks>
	public interface IDistributedLockService : IDependency {

        /// <summary>
        /// Tries to acquire a named distributed lock within the current tenant.
        /// </summary>
        /// <param name="name">The name of the lock to acquire.</param>
        /// <param name="maxValidFor">The maximum amount of time the lock should be held before automatically expiring. This is a safeguard in case the owner fails to release the lock. If <c>null</c> is specified, the lock never automatically expires.</param>
        /// <param name="timeout">The amount of time to wait for the lock to be acquired. Passing <c>TimeSpan.Zero</c> will cause the method to return immediately. Passing <c>null</c> will cause the method to block indefinitely until a lock can be acquired.</param>
        /// <param name="lock">This out parameter will be assigned the acquired lock if successful.</param>
        /// <returns><c>true</c> if the lock was successfully acquired, otherwise <c>false</c>.</returns>
        bool TryAcquireLock(string name, TimeSpan? maxValidFor, TimeSpan? timeout, out IDistributedLock @lock);

		/// <summary>
		/// Acquires a named distributed lock within the current tenant or throws if the lock cannot be acquired.
		/// </summary>
		/// <param name="name">The name of the lock to acquire.</param>
		/// <param name="maxValidFor">The maximum amount of time the lock should be held before automatically expiring. This is a safeguard in case the owner fails to release the lock. If <c>null</c> is specified, the lock never automatically expires.</param>
		/// <param name="timeout">The amount of time to wait for the lock to be acquired. Passing <c>TimeSpan.Zero</c> will cause the method to return immediately. Passing <c>null</c> will cause the method to block indefinitely until a lock can be acquired.</param>
		/// <returns>The acquired lock.</returns>
		/// <exception cref="TimeoutException">This method throws a TimeoutException if the lock could not be acquired within the specified timeout period.</exception>
		IDistributedLock AcquireLock(string name, TimeSpan? maxValidFor, TimeSpan? timeout);
    }

    public static class DistributedLockServiceExtensions {

		/// <summary>
		/// Tries to immediately acquire a named distributed lock with a given expiration time within the current tenant.
		/// </summary>
		/// <param name="name">The name of the lock to acquire.</param>
		/// <param name="maxValidFor">The maximum amount of time the lock should be held before automatically expiring. This is a safeguard in case the owner fails to release the lock. If <c>null</c> is specified, the lock never automatically expires.</param>
		/// <param name="lock">This out parameter will be assigned the acquired lock if successful.</param>
		/// <returns><c>true</c> if the lock could be immediately acquired, otherwise <c>false</c>.</returns>
		public static bool TryAcquireLock(this IDistributedLockService service, string name, TimeSpan? maxValidFor, out IDistributedLock @lock) {
            return service.TryAcquireLock(name, maxValidFor, TimeSpan.Zero, out @lock);
        }

		/// <summary>
		/// Tries to immediately acquire a named distributed lock with no expiration time within the current tenant.
		/// </summary>
		/// <param name="name">The name of the lock to acquire.</param>
		/// <param name="lock">This out parameter will be assigned the acquired lock if successful.</param>
		/// <returns><c>true</c> if the lock could be immediately acquired, otherwise <c>false</c>.</returns>
		public static bool TryAcquireLock(this IDistributedLockService service, string name, out IDistributedLock @lock) {
            return service.TryAcquireLock(name, null, TimeSpan.Zero, out @lock);
        }

		/// <summary>
		/// Acquires a named distributed lock with a given expiration time within the current tenant.
		/// </summary>
		/// <param name="name">The name of the lock to acquire.</param>
		/// <param name="maxValidFor">The maximum amount of time the lock should be held before automatically expiring. This is a safeguard in case the owner fails to release the lock. If <c>null</c> is specified, the lock never automatically expires.</param>
		/// <returns>The acquired lock.</returns>
		/// <remarks>This method blocks indefinitely until the lock can be acquired.</remarks>
		public static IDistributedLock AcquireLock(this IDistributedLockService service, string name, TimeSpan? maxValidFor) {
            return service.AcquireLock(name, maxValidFor, null);
        }

		/// <summary>
		/// Acquires a named distributed lock with no expiration time within the current tenant.
		/// </summary>
		/// <param name="name">The name to use for the lock.</param>
		/// <returns>The acquired lock.</returns>
		/// <remarks>This method blocks indefinitely until the lock can be acquired.</remarks>
		public static IDistributedLock AcquireLock(this IDistributedLockService service, string name) {
            return service.AcquireLock(name, null, null);
        }
    }
}