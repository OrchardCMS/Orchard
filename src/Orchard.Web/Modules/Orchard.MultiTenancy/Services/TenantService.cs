using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Extensions;

namespace Orchard.MultiTenancy.Services {
    public class TenantService : ITenantService {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IExtensionManager _extensionManager;

        public TenantService(
            IShellSettingsManager shellSettingsManager,
            IExtensionManager extensionManager) {
            _shellSettingsManager = shellSettingsManager;
            _extensionManager = extensionManager;
        }

        public IEnumerable<ShellSettings> GetTenants() {
            return _shellSettingsManager.LoadSettings();
        }

        public void CreateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
        }

        public void UpdateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
        }

        /// <summary>
        /// Loads only installed themes
        /// </summary>
        public IEnumerable<ExtensionDescriptor> GetInstalledThemes() {
            return GetThemes(_extensionManager.AvailableExtensions());
        }

        /// <summary>
        /// Loads only installed modules
        /// </summary>
        public IEnumerable<ExtensionDescriptor> GetInstalledModules() {
            return _extensionManager.AvailableExtensions().Where(descriptor => DefaultExtensionTypes.IsModule(descriptor.ExtensionType));
        }

        private IEnumerable<ExtensionDescriptor> GetThemes(IEnumerable<ExtensionDescriptor> extensions) {
            var themes = new List<ExtensionDescriptor>();
            foreach (var descriptor in extensions) {

                if (!DefaultExtensionTypes.IsTheme(descriptor.ExtensionType)) {
                    continue;
                }

                ExtensionDescriptor theme = descriptor;

                if (theme.Tags == null || !theme.Tags.Contains("hidden")) {
                    themes.Add(theme);
                }
            }
            return themes;
        }
    }
}