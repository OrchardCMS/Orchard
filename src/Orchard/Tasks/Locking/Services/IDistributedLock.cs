using System;

namespace Orchard.Tasks.Locking.Services {

    /// <summary>
    /// Represents a distributed lock returned by <c>IDistributedLockService</c>. The owner of the 
	/// lock should call <c>Dispose()</c> on an instance of this interface to release the distributed
	/// lock.
    /// </summary>
    public interface IDistributedLock : IDisposable {
        string Name { get; }
    }
}