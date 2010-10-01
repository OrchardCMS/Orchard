using System;
using System.Linq;
using Orchard.Commands;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Migration.Interpreters;
using Orchard.Environment.Extensions;

namespace Orchard.Migrations.Commands {

    [OrchardFeature("Orchard.Migration")]
    public class DataMigrationCommands : DefaultOrchardCommandHandler {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IDataMigrationInterpreter _dataMigrationInterpreter;
        private readonly ISchemaCommandGenerator _schemaCommandGenerator;

        public DataMigrationCommands(
            IDataMigrationManager dataMigrationManager,
            IDataMigrationInterpreter dataMigrationInterpreter,
            ISchemaCommandGenerator schemaCommandGenerator
            ) {
            _dataMigrationManager = dataMigrationManager;
            _dataMigrationInterpreter = dataMigrationInterpreter;
            _schemaCommandGenerator = schemaCommandGenerator;
        }

        [OrchardSwitch]
        public bool Drop { get; set; }

        [CommandName("upgrade database")]
        [CommandHelp("upgrade database <feature-name> \r\n\t" + "Upgrades or create the database tables for the <feature-name>")]
        public string UpgradeDatabase(string featureName) {
            try {
                _dataMigrationManager.Update(featureName);
            }
            catch ( Exception ex ) {
                Context.Output.WriteLine(T("An error occured while upgrading the database: " + ex.Message));
                return "Upgrade terminated.";
            }

            return "Database upgraded";
        }

        [CommandName("update database")]
        [CommandHelp("update database \r\n\t" + "Automatically updates the database schema for the enabled features")]
        public string UpdateDatabase() {
            try {
                _schemaCommandGenerator.UpdateDatabase();
            }
            catch ( Exception ex ) {
                Context.Output.WriteLine(T("An error occured while updating the database: " + ex.Message));
                return "Update terminated.";
            }

            return "Database updated";
        }

        [CommandName("create tables")]
        [CommandHelp("create tables <feature-name> [/Drop:true|false] \r\n\t" + "Creates the database tables for the <feature-name> and optionally drops them before if specified")]
        [OrchardSwitches("Drop")]
        public string CreateTables(string featureName) {
            var stringInterpreter = new StringCommandInterpreter(Context.Output);
            try {
                var commands = _schemaCommandGenerator.GetCreateFeatureCommands(featureName, Drop).ToList();
                if ( commands.Any() ) {

                    foreach (var command in commands) {
                        stringInterpreter.Visit(command);
                        _dataMigrationInterpreter.Visit(command);
                    }
                }
                else {
                    return "There are no tables to create for this feature.";
                }
            }
            catch ( Exception ex ) {
                Context.Output.WriteLine(T("An error occured while creating the tables: " + ex.Message));
                return "Tables creation terminated.";
            }

            return "Tables created";
        }
    }
}