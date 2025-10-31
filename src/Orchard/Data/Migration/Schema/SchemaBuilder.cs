using System;
using Orchard.Data.Migration.Interpreters;
using Orchard.Exceptions;
using Orchard.Localization;

namespace Orchard.Data.Migration.Schema
{
    public class SchemaBuilder
    {
        public Localizer T { get; set; }

        public SchemaBuilder(IDataMigrationInterpreter interpreter, string featurePrefix = null, Func<string, string> formatPrefix = null)
        {
            Interpreter = interpreter;
            FeaturePrefix = featurePrefix ?? string.Empty;
            FormatPrefix = formatPrefix ?? (s => s ?? string.Empty);
            T = NullLocalizer.Instance;
        }

        public IDataMigrationInterpreter Interpreter { get; }

        public string FeaturePrefix { get; }

        public Func<string, string> FormatPrefix { get; }

        /// <summary>
        /// Translate Table name into database table name - including prefixes.
        /// </summary>
        public virtual string TableDbName(string srcTable, string featurePrefixOverride = null)
        {
            return Interpreter.PrefixTableName(FormatPrefix(featurePrefixOverride ?? FeaturePrefix) + srcTable);
        }

        /// <summary>
        /// Removes the data table prefix from the specified table name.
        /// </summary>
        public virtual string RemoveDataTablePrefix(string prefixedTableName)
        {
            return Interpreter.RemovePrefixFromTableName(prefixedTableName);
        }

        public SchemaBuilder CreateTable(string name, Action<CreateTableCommand> table)
        {
            var createTable = new CreateTableCommand(string.Concat(FormatPrefix(FeaturePrefix), name));
            table(createTable);
            Run(createTable);
            return this;
        }

        public SchemaBuilder AlterTable(string name, Action<AlterTableCommand> table)
        {
            var alterTable = new AlterTableCommand(string.Concat(FormatPrefix(FeaturePrefix), name));
            table(alterTable);
            Run(alterTable);
            return this;
        }

        public SchemaBuilder DropTable(string name)
        {
            var deleteTable = new DropTableCommand(string.Concat(FormatPrefix(FeaturePrefix), name));
            Run(deleteTable);
            return this;
        }

        public SchemaBuilder ExecuteSql(string sql, Action<SqlStatementCommand> statement = null)
        {
            try
            {
                var sqlStatmentCommand = new SqlStatementCommand(sql);
                if (statement != null)
                {
                    statement(sqlStatmentCommand);
                }
                Run(sqlStatmentCommand);
                return this;
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }
                throw new OrchardException(T("An unexpected error occurred while executing the SQL statement: {0}", sql), ex); // Add the sql to the nested exception information
            }
        }

        private void Run(ISchemaBuilderCommand command)
        {
            Interpreter.Visit(command);
        }

        public SchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        {
            var command = new CreateForeignKeyCommand(name, string.Concat(FormatPrefix(FeaturePrefix), srcTable), srcColumns, string.Concat(FormatPrefix(FeaturePrefix), destTable), destColumns);
            Run(command);
            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcModule, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        {
            var command = new CreateForeignKeyCommand(name, string.Concat(FormatPrefix(srcModule), srcTable), srcColumns, string.Concat(FormatPrefix(FeaturePrefix), destTable), destColumns);
            Run(command);
            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destModule, string destTable, string[] destColumns)
        {
            var command = new CreateForeignKeyCommand(name, string.Concat(FormatPrefix(FeaturePrefix), srcTable), srcColumns, string.Concat(FormatPrefix(destModule), destTable), destColumns);
            Run(command);
            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcModule, string srcTable, string[] srcColumns, string destModule, string destTable, string[] destColumns)
        {
            var command = new CreateForeignKeyCommand(name, string.Concat(FormatPrefix(srcModule), srcTable), srcColumns, string.Concat(FormatPrefix(destModule), destTable), destColumns);
            Run(command);
            return this;
        }

        public SchemaBuilder DropForeignKey(string srcTable, string name)
        {
            var command = new DropForeignKeyCommand(string.Concat(FormatPrefix(FeaturePrefix), srcTable), name);
            Run(command);
            return this;
        }

        public SchemaBuilder DropForeignKey(string srcModule, string srcTable, string name)
        {
            var command = new DropForeignKeyCommand(string.Concat(FormatPrefix(srcModule), srcTable), name);
            Run(command);
            return this;
        }

    }
}
