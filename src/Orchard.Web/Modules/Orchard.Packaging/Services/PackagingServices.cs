using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Localization;

namespace Orchard.Packaging.Services {
    /// <summary>
    /// Provides generic packaging related methods.
    /// </summary>
    public class PackagingServices : IPackagingServices {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;

        public PackagingServices(IOrchardServices orchardServices,
            ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Verifies if the current user is allowed to manage packages. The super user of the default tenant site is always allowed.
        /// </summary>
        /// <returns>True if the allowed; False otherwise.</returns>
        public bool CanManagePackages() {
            // Check if super user for default tenant site
            if (_shellSettings.Name == ShellSettings.DefaultName
                && _orchardServices.WorkContext.CurrentUser.UserName == _orchardServices.WorkContext.CurrentSite.SuperUser) {

                return true;
            }

            // Check if it has permission explicitly assigned
            return _orchardServices.Authorizer.Authorize(Permissions.ManagePackages, T("Not authorized to manage packages."));
        }
    }
}