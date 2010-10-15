using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Migration.Interpreters;
using Orchard.Environment.Extensions;

namespace Orchard.Migrations.Commands {

    [OrchardFeature("Orchard.Migrations")]
    public class DataMigrationCommands : DefaultOrchardCommandHandler {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IExtensionManager _extensionManager;

        public DataMigrationCommands(
            IDataMigrationManager dataMigrationManager,
            IExtensionManager extensionManager
            ) {
            _dataMigrationManager = dataMigrationManager;
            _extensionManager = extensionManager;
        }

        [CommandName("upgrade database")]
        [CommandHelp("upgrade database <feature-name-1> ... <feature-name-n> \r\n\t" + "Upgrades or create the database tables for the <feature-name> or all features if not available")]
        public string UpgradeDatabase(params string[] featureNames) {
            try {
                IEnumerable<string> features = featureNames.Any()
                                                   ? featureNames
                                                   : _extensionManager.AvailableExtensions()
                                                         .SelectMany(ext => ext.Features)
                                                         .Select(f => f.Name);

                foreach(var feature in features) {
                    _dataMigrationManager.Update(feature);    
                }
            }
            catch ( Exception ex ) {
                Context.Output.WriteLine(T("An error occured while upgrading the database: " + ex.Message));
                return "Upgrade terminated.";
            }

            return "Database upgraded";
        }
    }
    [OrchardFeature("DatabaseUpdate")]
    public class DatabaseUpdateCommands : DefaultOrchardCommandHandler {
        private readonly IDataMigrationInterpreter _dataMigrationInterpreter;
        private readonly ISchemaCommandGenerator _schemaCommandGenerator;

        [OrchardSwitch]
        public bool Drop { get; set; }

        public DatabaseUpdateCommands(
            IDataMigrationInterpreter dataMigrationInterpreter,
            ISchemaCommandGenerator schemaCommandGenerator
            ) {
            _dataMigrationInterpreter = dataMigrationInterpreter;
            _schemaCommandGenerator = schemaCommandGenerator;
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