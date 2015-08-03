using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Logging;

namespace Orchard.ImportExport.Services {
    public class DatabaseManager : Component, IDatabaseManager {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IDataMigrationInterpreter _dataMigrationInterpreter;

        public DatabaseManager(
            ISessionFactoryHolder sessionFactoryHolder, 
            IDataMigrationInterpreter dataMigrationInterpreter) {

            _sessionFactoryHolder = sessionFactoryHolder;
            _dataMigrationInterpreter = dataMigrationInterpreter;
        }

        public IEnumerable<string> GetTenantDatabaseTableNames() {
            var configuration = _sessionFactoryHolder.GetConfiguration();
            var result = configuration.ClassMappings.Select(x => x.Table.Name);
            return result.ToArray();
        }

        public void DropTenantDatabaseTables() {
            var tableNames = GetTenantDatabaseTableNames();
            var schemaBuilder = new SchemaBuilder(_dataMigrationInterpreter);

            foreach (var tableName in tableNames) {
                try {
                    schemaBuilder.DropTable(schemaBuilder.RemoveDataTablePrefix(tableName));
                }
                catch (Exception ex) {
                    Logger.Warning(ex, "Failed to drop table '{0}'.", tableName);
                }
            }
        }
    }
}