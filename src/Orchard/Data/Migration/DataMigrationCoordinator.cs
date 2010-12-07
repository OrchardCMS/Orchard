using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Data.Migration {
    /// <summary>
    /// Responsible for executing data migration tasks when a feature is enabled for the first time
    /// 1) Initial install of a module:
    ///     Enable a feature for the first time => run data migration up to the latest version
    ///     Enable a feature on 2nd time => no data migration run
    ///     Disable a feature => no data migration
    /// 2) Installing a newer version of a module
    /// Don't do any data migration by default
    /// 2 cases: 
    ///     1) feature wasn't not enabled when new code was installed
    ///     2) feature was enabled when new code was installed
    /// </summary>
    public class DataMigrationCoordinator : IFeatureEventHandler {
        private readonly IDataMigrationManager _dataMigrationManager;

        public DataMigrationCoordinator(IDataMigrationManager dataMigrationManager) {
            _dataMigrationManager = dataMigrationManager;
        }

        public void Installing(Feature feature) {
            var featureName = feature.Descriptor.Id;
            _dataMigrationManager.Update(featureName);
        }

        public void Installed(Feature feature) {
        }

        public void Enabling(Feature feature) {
        }

        public void Enabled(Feature feature) {
        }

        public void Disabling(Feature feature) {
        }

        public void Disabled(Feature feature) {
        }

        public void Uninstalling(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
            var featureName = feature.Descriptor.Id;
            if ( _dataMigrationManager.IsFeatureAlreadyInstalled(featureName) ) {
                _dataMigrationManager.Uninstall(featureName);
            }
        }
    }
}
