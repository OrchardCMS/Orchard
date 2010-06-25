using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DataMigration {
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

        public void Install(Feature feature) {
            var featureName = feature.Descriptor.Name;
            if ( !_dataMigrationManager.IsFeatureAlreadyInstalled(featureName) ) {
                _dataMigrationManager.Update(featureName);
            }
        }

        public void Enable(Feature feature) {
        }

        public void Disable(Feature feature) {
        }

        public void Uninstall(Feature feature) {
            var featureName = feature.Descriptor.Name;
            if ( _dataMigrationManager.IsFeatureAlreadyInstalled(featureName) ) {
                _dataMigrationManager.Uninstall(featureName);
            }
        }
    }
}
