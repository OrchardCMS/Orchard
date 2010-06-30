using System;
using Orchard.Commands;

namespace Orchard.Data.Migration.Commands {
    public class DataMigrationCommands : DefaultOrchardCommandHandler {
        private readonly IDataMigrationManager _dataMigrationManager;

        public DataMigrationCommands(
            IDataMigrationManager dataMigrationManager) {
            _dataMigrationManager = dataMigrationManager;
        }

        [OrchardSwitch]
        public string Feature { get; set; }

        [CommandName("upgrade database")]
        [CommandHelp("upgrade database /Feature:<feature> \r\n\t" + "Upgrades or create the database tables for the named <feature>")]
        [OrchardSwitches("Feature")]
        public string UpgradeDatabase() {
            try {
                _dataMigrationManager.Update(Feature);
            }
            catch ( Exception ex ) {
                Context.Output.WriteLine(T("An error occured while upgrading the database: " + ex.Message));
                return "Upgrade terminated.";
            }

            return "Database upgraded";
        }
    }
}