namespace Orchard.Packaging.Services {
    /// <summary>
    /// Provides generic packaging related methods.
    /// </summary>
    public interface IPackagingServices : IDependency {

        /// <summary>
        /// Verifies if the current user is allowed to manage packages. The super user of the default tenant site is always allowed.
        /// </summary>
        /// <returns>True if the allowed; false otherwise.</returns>
        bool CanManagePackages();
    }
}