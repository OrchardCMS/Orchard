using System;
using System.Linq;
using Orchard.Commands;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Migration.Interpreters;
using Orchard.Environment.Extensions;

namespace Orchard.Migrations.Commands {
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
        [CommandHelp("update database \r\n\t" + "Automatically updates the database schema according to the defintion of the \"Record\" types in code for the enabled features.")]
        public void UpdateDatabase() {
            try {
                _schemaCommandGenerator.UpdateDatabase();
            }
            catch ( Exception ex ) {
                throw new OrchardException(T("An error occured while updating the database."), ex);
            }

            Context.Output.WriteLine(T("Database updated"));
        }

        [CommandName("create tables")]
        [CommandHelp("create tables <feature-name> [/Drop:true|false] \r\n\t" + "Creates the database tables according to the defintion of the \"Record\" types in code for the <feature-name> and optionally drops them before if specified.")]
        [OrchardSwitches("Drop")]
        public void CreateTables(string featureName) {
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
                    Context.Output.WriteLine(T("There are no tables to create for feature {0}.", featureName));
                    return;
                }
            }
            catch ( Exception ex ) {
                throw new OrchardException(T("An error occured while creating the tables."), ex);
            }

            Context.Output.WriteLine(T("Tables created"));
        }
    }
}