using System.Collections.Generic;
using System.Linq;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Events;

namespace Orchard.MultiTenancy.Services {
    public class TenantService : ITenantService {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsEventHandler _shellSettingsEventHandler;

        public TenantService(IShellSettingsManager shellSettingsManager, IOrchardHost orchardHost, IShellSettingsEventHandler shellSettingsEventHandler) {
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
            _shellSettingsEventHandler = shellSettingsEventHandler;
        }

        public IEnumerable<ShellSettings> GetTenants() {
            return _shellSettingsManager.LoadSettings();
        }

        public void CreateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
            _shellSettingsEventHandler.Created(settings);
        }

        public void UpdateTenant(ShellSettings settings) {
            var tenant = GetTenants().FirstOrDefault(ss => ss.Name == settings.Name);
            if ( tenant != null ) {
                _shellSettingsManager.SaveSettings(settings);
                _shellSettingsEventHandler.Updated(settings);
            }

        }
    }
}