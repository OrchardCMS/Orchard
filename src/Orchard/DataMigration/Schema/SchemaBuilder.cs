using System;
using System.Collections.Generic;

namespace Orchard.DataMigration.Schema {
    public class SchemaBuilder {

        private readonly List<SchemaCommand> _schemaCommands;

        public SchemaBuilder() {
            _schemaCommands = new List<SchemaCommand>();
        }

        public SchemaBuilder(string tablePrefix) : this() {
            TablePrefix = tablePrefix;
        }

        public string TablePrefix { get; private set; }

        public SchemaBuilder CreateTable(string name, Action<CreateTableCommand> table) {
            var createTable = new CreateTableCommand(name);
            table(createTable);
            _schemaCommands.Add(createTable);
            return this;
        }

        public SchemaBuilder AlterTable(string name, Action<AlterTableCommand> table) {
            var alterTable = new AlterTableCommand(name);
            table(alterTable);
            _schemaCommands.Add(alterTable);
            return this;
        }

        public SchemaBuilder DropTable(string name) {
            var deleteTable = new DropTableCommand(name);
            _schemaCommands.Add(deleteTable);
            return this;
        }

        public SchemaBuilder ExecuteSql(string sql, Action<SqlStatementCommand> statement) {
            var sqlStatmentCommand = new SqlStatementCommand(sql);
            statement(sqlStatmentCommand);
            return this;
        }

    }
}
