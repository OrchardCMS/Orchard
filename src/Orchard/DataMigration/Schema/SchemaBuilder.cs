using System;
using Orchard.DataMigration.Interpreters;

namespace Orchard.DataMigration.Schema {
    public class SchemaBuilder {
        private readonly IDataMigrationInterpreter _interpreter;
        public SchemaBuilder(IDataMigrationInterpreter interpreter) {
            _interpreter = interpreter;
        }

        public SchemaBuilder CreateTable(string name, Action<CreateTableCommand> table) {
            var createTable = new CreateTableCommand(name);
            table(createTable);
            Run(createTable);
            return this;
        }

        public SchemaBuilder AlterTable(string name, Action<AlterTableCommand> table) {
            var alterTable = new AlterTableCommand(name);
            table(alterTable);
            Run(alterTable);
            return this;
        }

        public SchemaBuilder DropTable(string name) {
            var deleteTable = new DropTableCommand(name);
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

        private void Run(SchemaCommand command) {
            _interpreter.Visit(command);
        }

        public SchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns) {
            var command = new CreateForeignKeyCommand(name, srcTable, srcColumns, destTable, destColumns);
            Run(command);
            return this;
        }

        public SchemaBuilder DropForeignKey(string srcTable, string name) {
            var command = new DropForeignKeyCommand(srcTable, name);
            Run(command);
            return this;
        }

    }
}
