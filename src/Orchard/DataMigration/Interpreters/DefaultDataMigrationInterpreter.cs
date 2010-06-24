using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using Orchard.Data;
using Orchard.DataMigration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Orchard.DataMigration.Interpreters {
    public class DefaultDataMigrationInterpreter : IDataMigrationInterpreter {
        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<ICommandInterpreter> _commandInterpreters;
        private readonly ISession _session;
        private readonly Dialect _dialect;
        private readonly List<string> _sqlStatements;
        private const char Space = ' ' ;

        public DefaultDataMigrationInterpreter(ShellSettings shellSettings, ISessionLocator sessionLocator, IEnumerable<ICommandInterpreter> commandInterpreters) {
            _shellSettings = shellSettings;
            _commandInterpreters = commandInterpreters;
            _session = sessionLocator.For(typeof(DefaultDataMigrationInterpreter));
            _sqlStatements = new List<string>();
            _dialect = _shellSettings.DataProvider == "SQLite" ? (Dialect) new SQLiteDialect() : new MsSql2008Dialect();
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<string> SqlStatements {
            get { return _sqlStatements; }
        }

        public void Visit(SchemaCommand command) {
            switch (command.Type) {
                case SchemaCommandType.CreateTable:
                    Visit((CreateTableCommand)command);
                    break;
                case SchemaCommandType.AlterTable:
                    Visit((AlterTableCommand)command);
                    break;
                case SchemaCommandType.DropTable:
                    Visit((DropTableCommand)command);
                    break;
                case SchemaCommandType.SqlStatement:
                    Visit((SqlStatementCommand)command);
                    break;
                case SchemaCommandType.CreateForeignKey:
                    Visit((CreateForeignKeyCommand)command);
                    break;
                case SchemaCommandType.DropForeignKey:
                    Visit((DropForeignKeyCommand)command);
                    break;
            }
        }

        public void Visit(CreateTableCommand command) {

            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append(_dialect.CreateMultisetTableString)
                .Append(' ')
                .Append(_dialect.QuoteForTableName(_shellSettings.DataTablePrefix + command.Name))
                .Append(" (");

            var appendComma = false;
            foreach(var createColumn in command.TableCommands.OfType<CreateColumnCommand>()) {
                if(appendComma) {
                    builder.Append(", ");
                }
                appendComma = true;
                
                Visit(builder, createColumn);
            }

            builder.Append(" )");
            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public void Visit(DropTableCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append(_dialect.GetDropTableString(_shellSettings.DataTablePrefix + command.Name));
            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public void Visit(AlterTableCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            if(command.TableCommands.Count == 0) {
                return;
            }

            // drop columns
            foreach ( var dropColumn in command.TableCommands.OfType<DropColumnCommand>() ) {
                var builder = new StringBuilder();
                Visit(builder, dropColumn);
                RunPendingStatements();
            }

            // add columns
            foreach ( var addColumn in command.TableCommands.OfType<AddColumnCommand>() ) {
                var builder = new StringBuilder();
                Visit(builder, addColumn);
                RunPendingStatements();
            }

            // alter columns
            foreach ( var alterColumn in command.TableCommands.OfType<AlterColumnCommand>() ) {
                var builder = new StringBuilder();
                Visit(builder, alterColumn);
                RunPendingStatements();
            }
        }

        public void Visit(StringBuilder builder, AddColumnCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            builder.AppendFormat("alter table {0} add column ", _dialect.QuoteForTableName(_shellSettings.DataTablePrefix + command.TableName));

            Visit(builder, (CreateColumnCommand)command);
            _sqlStatements.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder, DropColumnCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            builder.AppendFormat("alter table {0} drop column {1}", 
                _dialect.QuoteForTableName(_shellSettings.DataTablePrefix + command.TableName),
                _dialect.QuoteForColumnName(command.TableName));
            _sqlStatements.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder,  AlterColumnCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            builder.AppendFormat("alter table {0} alter column {1} ",
                _dialect.QuoteForTableName(_shellSettings.DataTablePrefix + command.TableName),
                _dialect.QuoteForColumnName(command.TableName));

            // type
            if ( command.DbType != DbType.Object ) {
                builder.Append(GetTypeName(command.DbType, command.Length, command.Precision, command.Scale));
            }

            // [default value]
            if ( !string.IsNullOrEmpty(command.Default) ) {
                builder.Append(" default ").Append(command.Default).Append(Space);
            }
            _sqlStatements.Add(builder.ToString());
        }

        public void Visit(SqlStatementCommand command) {
            if (command.Providers.Count == 0 || command.Providers.Contains(_shellSettings.DataProvider) ) {
                if (ExecuteCustomInterpreter(command)) {
                    return;
                }
                _sqlStatements.Add(command.Sql);

                RunPendingStatements();
            }
        }

        public void Visit(CreateForeignKeyCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialect.QuoteForTableName(_shellSettings.DataTablePrefix + command.SrcTable));

            builder.Append(_dialect.GetAddForeignKeyConstraintString(command.Name,
                command.SrcColumns,
                command.DestTable,
                command.DestColumns,
                false));

            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public void Visit(DropForeignKeyCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            var builder = new StringBuilder();

            builder.AppendFormat("alter table {0} drop constraint {1}", command.SrcTable, command.Name);
            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        private string GetTypeName(DbType dbType, int? length, byte precision, byte scale) {
            return precision > 0
                       ? _dialect.GetTypeName(new SqlType(dbType, precision, scale))
                       : length.HasValue
                             ? _dialect.GetTypeName(new SqlType(dbType, length.Value))
                             : _dialect.GetTypeName(new SqlType(dbType));
        }

        private void Visit(StringBuilder builder, CreateColumnCommand command) {
            if ( ExecuteCustomInterpreter(command) ) {
                return;
            }

            // name
            builder.Append(_dialect.QuoteForColumnName(command.ColumnName)).Append(Space);

            // type
            builder.Append(GetTypeName(command.DbType, command.Length, command.Precision, command.Scale));

            // [default value]
            if ( !string.IsNullOrEmpty(command.Default) ) {
                builder.Append(" default ").Append(command.Default).Append(Space);
            }

            // nullable
            builder.Append(command.IsNotNull
                               ? " not null"
                               : !command.IsPrimaryKey && !command.IsUnique
                                     ? _dialect.NullColumnString
                                     : string.Empty);

            // append unique if handled, otherwise at the end of the satement
            if ( command.IsUnique && _dialect.SupportsUnique ) {
                builder.Append(" unique");
            }

            if ( command.IsPrimaryKey ) {
                builder.Append(Space).Append(_dialect.PrimaryKeyString);
            }
        }

        private void RunPendingStatements() {

            var connection = _session.Connection;

            foreach ( var sqlStatement in _sqlStatements ) {
                Logger.Debug(sqlStatement);
                using ( var command = connection.CreateCommand() ) {
                    command.CommandText = sqlStatement;
                    command.ExecuteNonQuery();
                }
            }

            _sqlStatements.Clear();
        }

        private bool ExecuteCustomInterpreter<T>(T command) where T : ISchemaBuilderCommand {
            var interpreter = _commandInterpreters
                .Where(ici => ici.DataProvider == _shellSettings.DataProvider)
                .OfType<ICommandInterpreter<T>>()
                .FirstOrDefault();

            if ( interpreter != null ) {
                _sqlStatements.AddRange(interpreter.CreateStatements(command));
                RunPendingStatements();
                return true;
            }

            return false;
        }
    }
}
