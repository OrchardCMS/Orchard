using System;
using System.Linq;
using Orchard.Commands;
using Orchard.Data.Migration;
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
        public void UpgradeDatabase(params string[] featureNames) {
            var features = featureNames.Any()
                                   ? featureNames
                                   : _extensionManager.AvailableExtensions()
                                         .SelectMany(ext => ext.Features)
                                         .Select(f => f.Id);
            try {
                foreach(var feature in features) {
                    _dataMigrationManager.Update(feature);    
                }
            }
            catch ( Exception ex ) {
                throw new OrchardException(T("An error occured while upgrading the database."), ex);
            }

            Context.Output.WriteLine(T("Database upgraded"));
        }
    }
}