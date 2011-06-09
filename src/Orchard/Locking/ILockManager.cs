using System;

namespace Orchard.Locking {
    /// <summary>
    /// Provides a mechanism that synchronizes access to named resources for a tenant.
    /// This service should be used when two or more processes might use a critical 
    /// resource at the tenant scope.
    /// For instance, multiple events might trigger a task which should be unique at a time. 
    /// In this case using ILockManager can prevent reentrancy.
    /// </summary>
    public interface ILockManager : ISingletonDependency {
        /// <summary>
        /// Attempts to acquire an exclusive lock on a specific resource. If the <paramref name="resourceKey"/>
        /// is already locked, this method will return <c>null</c>. Otherwise a lock is granted by returning an <c>IDisposable</c>
        /// instance, which is then disposed in order to release the lock.
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
