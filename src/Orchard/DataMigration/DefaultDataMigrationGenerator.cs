using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Cfg;
using Orchard.Data.Builders;
using Orchard.DataMigration.Schema;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.DataMigration {
    public class DefaultDataMigrationGenerator : IDataMigrationGenerator {
        public IEnumerable<ISchemaBuilderCommand> CreateCommands(IEnumerable<RecordBlueprint> records) {

            // use FluentNhibernate generation for this module
            var persistenceModel = AbstractBuilder.CreatePersistenceModel(records);
            var configuration = Fluently.Configure().Mappings(m => m.AutoMappings.Add(persistenceModel));
            
            return Enumerable.Empty<ISchemaBuilderCommand>();
        }
    }
}
