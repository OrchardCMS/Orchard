namespace Orchard.TaskLease.Services
{
    /// <summary>
    /// Describes a service which returns a name for the machine running the application.
    /// </summary>
    /// <remarks>
    /// Should be delocalized to IHostEnvironment in a leter version
    /// </remarks>
    public interface IMachineNameProvider : IDependency
    {
        /// <summary>
        /// Returns the current machine's name
        /// </summary>
        string GetMachineName();
    }
}