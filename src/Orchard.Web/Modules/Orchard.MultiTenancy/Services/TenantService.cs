using System.Collections.Generic;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Services {
    public class TenantService : ITenantService {
        private readonly IShellSettingsManager _shellSettingsManager;

        public TenantService(IShellSettingsManager shellSettingsManager) {
            _shellSettingsManager = shellSettingsManager;
        }

        #region Implementation of ITenantService

        public IEnumerable<ShellSettings> GetTenants() {
            return _shellSettingsManager.LoadSettings();
        }

        #endregion
    }
}