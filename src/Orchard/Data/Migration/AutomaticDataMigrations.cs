using System;
using System.Linq;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment;
using Orchard.Environment.Configuration;
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
        private readonly IDataMigrationInterpreter _dataMigrationInterpreter;
        private readonly ShellSettings _shellSettings;
        private readonly ITransactionManager _transactionManager;

        public AutomaticDataMigrations(
            IDataMigrationManager dataMigrationManager,
            IDataMigrationInterpreter dataMigrationInterpreter,
            IFeatureManager featureManager,
            IDistributedLockService distributedLockService,
            ITransactionManager transactionManager,
            ShellSettings shellSettings) {

            _dataMigrationManager = dataMigrationManager;
            _featureManager = featureManager;
            _distributedLockService = distributedLockService;
            _shellSettings = shellSettings;
            _transactionManager = transactionManager;
            _dataMigrationInterpreter = dataMigrationInterpreter;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Activated() {
            EnsureDistributedLockSchemaExists();

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

        /// <summary>
        /// This ensures that the framework migrations have run for the distributed locking feature, as existing Orchard installations will not have the required tables when upgrading.
        /// </summary>
        private void EnsureDistributedLockSchemaExists()
        {
            // Ensure the distributed lock record schema exists.
            var schemaBuilder = new SchemaBuilder(_dataMigrationInterpreter);
            var distributedLockSchemaBuilder = new DistributedLockSchemaBuilder(_shellSettings, schemaBuilder);
            if (!distributedLockSchemaBuilder.SchemaExists()) {

                // Workaround to avoid some Transaction issue for PostgreSQL.
                if (_shellSettings.DataProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)) {
                    _transactionManager.RequireNew();
                    distributedLockSchemaBuilder.CreateSchema();
                    return;
                }

                distributedLockSchemaBuilder.CreateSchema();
                _transactionManager.RequireNew();
            }
        }
    }
}
