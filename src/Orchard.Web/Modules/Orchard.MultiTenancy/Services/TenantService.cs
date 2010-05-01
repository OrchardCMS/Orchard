using System.Collections.Generic;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Services {
    public class TenantService : ITenantService {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;

        public TenantService(IShellSettingsManager shellSettingsManager, IOrchardHost orchardHost) {
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
        }

        public IEnumerable<ShellSettings> GetTenants() {
            return _shellSettingsManager.LoadSettings();
        }

        public void CreateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
        }
    }
}