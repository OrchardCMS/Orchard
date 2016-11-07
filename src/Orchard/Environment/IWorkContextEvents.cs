namespace Orchard.Environment {
    /// <summary>
    /// Describes events fired during work context lifetime.
    /// </summary>
    public interface IWorkContextEvents : IUnitOfWorkDependency {
        /// <summary>
        /// Fired when the work context is started.
        /// </summary>
        void Started();

        /// <summary>
        /// Fired when the work context is finished.
        /// </summary>
        void Finished();
    }
}