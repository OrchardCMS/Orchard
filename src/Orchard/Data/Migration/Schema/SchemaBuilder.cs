using System;
using Orchard.Data.Migration.Interpreters;

namespace Orchard.Data.Migration.Schema {
    public class SchemaBuilder {
        private readonly IDataMigrationInterpreter _interpreter;
        private readonly string _featurePrefix;

        public SchemaBuilder(IDataMigrationInterpreter interpreter, string featurePrefix = null) {
            _interpreter = interpreter;
            _featurePrefix = featurePrefix;
        }

        public SchemaBuilder CreateTable(string name, Action<CreateTableCommand> table) {
            var createTable = new CreateTableCommand(String.Concat(_featurePrefix, name));
            table(createTable);
            Run(createTable);
            return this;
        }

        public SchemaBuilder AlterTable(string name, Action<AlterTableCommand> table) {
            var alterTable = new AlterTableCommand(String.Concat(_featurePrefix, name));
            table(alterTable);
            Run(alterTable);
            return this;
        }

        public SchemaBuilder DropTable(string name) {
            var deleteTable = new DropTableCommand(String.Concat(_featurePrefix, name));
            Run(deleteTable);
            return this;
        }

        public SchemaBuilder ExecuteSql(string sql, Action<SqlStatementCommand> statement = null) {
            var sqlStatmentCommand = new SqlStatementCommand(sql);
            if ( statement != null ) {
                statement(sqlStatmentCommand);
            }
            Run(sqlStatmentCommand);
            return this;
        }

        private void Run(ISchemaBuilderCommand command) {
            _interpreter.Visit(command);
        }

        public SchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns) {
            var command = new CreateForeignKeyCommand(name, String.Concat(_featurePrefix, srcTable), srcColumns, String.Concat(_featurePrefix, destTable), destColumns);
            Run(command);
            return this;
        }

        public SchemaBuilder DropForeignKey(string srcTable, string name) {
            var command = new DropForeignKeyCommand(String.Concat(_featurePrefix, srcTable), name);
            Run(command);
            return this;
        }

    }
}
