using Orchard.Setup.Services;

namespace Orchard.ImportExport.Services {
    /// <summary>
    /// We need to manually instantiate the SetupService class because the Orchard.Setup feature will be disabled after setup completes.
    /// </summary>
    public interface ISetupServiceFactory : IDependency {
        ISetupService CreateSetupService();
    }
}