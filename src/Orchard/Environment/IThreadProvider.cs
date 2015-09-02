namespace Orchard.Environment {

    /// <summary>
    /// Describes a service which returns the managed thread ID of the current thread.
    /// </summary>
    public interface IThreadProvider {
    
        /// <summary>
        /// Returns the managed thread ID of the current thread.
        /// </summary>
        int GetCurrentThreadId();
    }
}