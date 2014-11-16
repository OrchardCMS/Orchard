namespace Orchard.TaskLease.Services {

    /// <summary>
    /// Describes a service which the name of the machine running the application.
    /// </summary>
    public interface IMachineNameProvider {
    
        /// <summary>
        /// Returns the name of the machine running the application.
        /// </summary>
        string GetMachineName();
    }
}