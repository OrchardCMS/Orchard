using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Services {
    public class TenantService : ITenantService {
        private readonly IShellSettingsManager _shellSettingsManager;

        public TenantService(IShellSettingsManager shellSettingsManager) {
            _shellSettingsManager = shellSettingsManager;
        }

        public IEnumerable<ShellSettings> GetTenants() {
            return _shellSettingsManager.LoadSettings();
        }

        public void CreateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
        }

        public void UpdateTenant(ShellSettings settings) {
            var tenant = GetTenants().FirstOrDefault(ss => ss.Name == settings.Name);
            if ( tenant != null ) {
                _shellSettingsManager.SaveSettings(settings);
            }
        }
    }
}