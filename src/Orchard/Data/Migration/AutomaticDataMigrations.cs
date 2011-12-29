using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.Data.Migration {
    /// <summary>
    /// Registers to OrchardShell.Activated in order to run migrations automatically 
    /// </summary>
    public class AutomaticDataMigrations : IOrchardShellEvents {
        private readonly IDataMigrationManager _dataMigrationManager;

        public AutomaticDataMigrations(IDataMigrationManager dataMigrationManager) {
            _dataMigrationManager = dataMigrationManager;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; } 

        public void Activated() {
            foreach (var feature in _dataMigrationManager.GetFeaturesThatNeedUpdate()) {
                try {
                    _dataMigrationManager.Update(feature);
                }
                catch (Exception e) {
                    Logger.Error("Could not run migrations automatically on " + feature, e);
                }
            }
        }

        public void Terminating() {
            
        }
    }
}
