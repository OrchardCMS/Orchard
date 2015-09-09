using System;
using System.Linq;
using Orchard.Environment;
using Orchard.Environment.Features;
using Orchard.Logging;
using Orchard.Tasks.Locking.Services;
using Orchard.Exceptions;

namespace Orchard.Data.Migration {
    /// <summary>
    /// Registers to OrchardShell.Activated in order to run migrations automatically 
    /// </summary>
    public class AutomaticDataMigrations : IOrchardShellEvents {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IFeatureManager _featureManager;
        private readonly IDistributedLockService _distributedLockService;

        public AutomaticDataMigrations(
            IDataMigrationManager dataMigrationManager,
            IFeatureManager featureManager,
            IDistributedLockService distributedLockService) {

            _dataMigrationManager = dataMigrationManager;
            _featureManager = featureManager;
            _distributedLockService = distributedLockService;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Activated() {
            IDistributedLock @lock;
            if (_distributedLockService.TryAcquireLock(GetType().FullName, TimeSpan.FromMinutes(30), TimeSpan.FromMilliseconds(250), out @lock)) {
                using (@lock) {
                    // Let's make sure that the basic set of features is enabled.  If there are any that are not enabled, then let's enable them first.
                    var theseFeaturesShouldAlwaysBeActive = new[] {
                        "Common", "Containers", "Contents", "Dashboard", "Feeds", "Navigation", "Scheduling", "Settings", "Shapes", "Title"
                    };

                    var enabledFeatures = _featureManager.GetEnabledFeatures().Select(f => f.Id).ToList();
                    var featuresToEnable = theseFeaturesShouldAlwaysBeActive.Where(shouldBeActive => !enabledFeatures.Contains(shouldBeActive)).ToList();
                    if (featuresToEnable.Any()) {
                        _featureManager.EnableFeatures(featuresToEnable, true);
                    }

                    foreach (var feature in _dataMigrationManager.GetFeaturesThatNeedUpdate()) {
                        try {
                            _dataMigrationManager.Update(feature);
                        }
                        catch (Exception ex) {
                            if (ex.IsFatal()) {
                                throw;
                            }
                            Logger.Error("Could not run migrations automatically on " + feature, ex);
                        }
                    }
                }
            }
        }

        public void Terminating() {
            // No-op.
        }
    }
}
