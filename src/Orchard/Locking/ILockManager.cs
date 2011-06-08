using System;

namespace Orchard.Locking {
    /// <summary>
    /// Abstraction for locks creation.
    /// </summary>
    public interface ILockManager : ISingletonDependency {
        /// <summary>
        /// Attempts to acquire an exclusive lock on a specific resource.
        /// </summary>
        /// <param name="resourceKey">The resource key of the lock to create.</param>
        /// <returns>A reference to the lock object if the lock is granted; otherwise <c>null</c>.</returns>
        /// <remarks>The lock instance has to be disposed in order to be released.</remarks>
        IDisposable Lock(string resourceKey);
        
        /// <summary>
        /// Whether a lock is already existing or not.
        /// </summary>
        /// <param name="resourceKey">The resource key of the lock to test.</param>
        /// <returns><c>true</c> if a lock exists; otherwise, <c>false</c>.</returns>
        bool IsLocked(string resourceKey);
    }
}
