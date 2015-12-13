using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using Orchard.ContentManagement.Records;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Reports.Services;

namespace Orchard.Data.Migration.Interpreters {
    public class DefaultDataMigrationInterpreter : AbstractDataMigrationInterpreter, IDataMigrationInterpreter {
        private readonly ShellSettings _shellSettings;
        private readonly ITransactionManager _transactionManager;
        private readonly IEnumerable<ICommandInterpreter> _commandInterpreters;
        private readonly Lazy<Dialect> _dialectLazy;
        private readonly List<string> _sqlStatements;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IReportsCoordinator _reportsCoordinator;

        private const char Space = ' ';

        public DefaultDataMigrationInterpreter(
            ShellSettings shellSettings,
            ITransactionManager ITransactionManager,
            IEnumerable<ICommandInterpreter> commandInterpreters,
            ISessionFactoryHolder sessionFactoryHolder,
            IReportsCoordinator reportsCoordinator) {
            _shellSettings = shellSettings;
            _transactionManager = ITransactionManager;
            _commandInterpreters = commandInterpreters;
            _sqlStatements = new List<string>();
            _sessionFactoryHolder = sessionFactoryHolder;
            _reportsCoordinator = reportsCoordinator;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _dialectLazy = new Lazy<Dialect>(() => Dialect.GetDialect(sessionFactoryHolder.GetConfiguration().Properties));
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<string> SqlStatements {
            get { return _sqlStatements; }
        }

        public override void Visit(CreateTableCommand command) {

            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append(_dialectLazy.Value.CreateMultisetTableString)
                .Append(' ')
                .Append(_dialectLazy.Value.QuoteForTableName(PrefixTableName(command.Name)))
                .Append(" (");

            var appendComma = false;
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>()) {
                if (appendComma) {
                    builder.Append(", ");
                }
                appendComma = true;

                Visit(builder, createColumn);
            }

            var primaryKeys = command.TableCommands.OfType<CreateColumnCommand>().Where(ccc => ccc.IsPrimaryKey).Select(ccc => ccc.ColumnName).ToArray();
            if (primaryKeys.Any()) {
                if (appendComma) {
                    builder.Append(", ");
                }

                builder.Append(_dialectLazy.Value.PrimaryKeyString)
                    .Append(" ( ")
                    .Append(String.Join(", ", primaryKeys.ToArray()))
                    .Append(" )");
            }

            builder.Append(" )");
            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public string PrefixTableName(string tableName) {
            if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
                return tableName;
            return _shellSettings.DataTablePrefix + "_" + tableName;
        }

        public override void Visit(DropTableCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append(_dialectLazy.Value.GetDropTableString(PrefixTableName(command.Name)));
            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public override void Visit(AlterTableCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            if (command.TableCommands.Count == 0) {
                return;
            }

            // drop columns
            foreach (var dropColumn in command.TableCommands.OfType<DropColumnCommand>()) {
                var builder = new StringBuilder();
                Visit(builder, dropColumn);
                RunPendingStatements();
            }

            // add columns
            foreach (var addColumn in command.TableCommands.OfType<AddColumnCommand>()) {
                var builder = new StringBuilder();
                Visit(builder, addColumn);
                RunPendingStatements();
            }

            // alter columns
            foreach (var alterColumn in command.TableCommands.OfType<AlterColumnCommand>()) {
                var builder = new StringBuilder();
                Visit(builder, alterColumn);
                RunPendingStatements();
            }

            // add index
            foreach (var addIndex in command.TableCommands.OfType<AddIndexCommand>()) {
                var builder = new StringBuilder();
                Visit(builder, addIndex);
                RunPendingStatements();
            }

            // drop index
            foreach (var dropIndex in command.TableCommands.OfType<DropIndexCommand>()) {
                var builder = new StringBuilder();
                Visit(builder, dropIndex);
                RunPendingStatements();
            }

        }

        public void Visit(StringBuilder builder, AddColumnCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            builder.AppendFormat("alter table {0} add ", _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)));

            Visit(builder, (CreateColumnCommand)command);
            _sqlStatements.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder, DropColumnCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            builder.AppendFormat("alter table {0} drop column {1}",
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialectLazy.Value.QuoteForColumnName(command.ColumnName));
            _sqlStatements.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder, AlterColumnCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            builder.AppendFormat("alter table {0} alter column {1} ",
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialectLazy.Value.QuoteForColumnName(command.ColumnName));

            // type
            if (command.DbType != DbType.Object) {
                builder.Append(GetTypeName(_dialectLazy.Value, command.DbType, command.Length, command.Precision, command.Scale));
            }
            else {
                if(command.Length > 0 || command.Precision > 0 || command.Scale > 0) {
                    throw new OrchardException(T("Error while executing data migration: you need to specify the field's type in order to change its properties"));
                }
            }

            // [default value]
            if (command.Default != null) {
                builder.Append(" set default ").Append(ConvertToSqlValue(command.Default)).Append(Space);
            }
            _sqlStatements.Add(builder.ToString());
        }


        public void Visit(StringBuilder builder, AddIndexCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            builder.AppendFormat("create index {1} on {0} ({2}) ",
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialectLazy.Value.QuoteForColumnName(PrefixTableName(command.IndexName)),
                String.Join(", ", command.ColumnNames));

            _sqlStatements.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder, DropIndexCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            builder.AppendFormat("drop index {0} ON {1}",
                _dialectLazy.Value.QuoteForColumnName(PrefixTableName(command.IndexName)),
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)));
            _sqlStatements.Add(builder.ToString());
        }

        public override void Visit(SqlStatementCommand command) {
            if (command.Providers.Count != 0 && !command.Providers.Contains(_shellSettings.DataProvider)) {
                return;
            }

            if (ExecuteCustomInterpreter(command)) {
                return;
            }
            _sqlStatements.Add(command.Sql);

            RunPendingStatements();
        }

        public override void Visit(CreateForeignKeyCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialectLazy.Value.QuoteForTableName(PrefixTableName(command.SrcTable)));

            builder.Append(_dialectLazy.Value.GetAddForeignKeyConstraintString(PrefixTableName(command.Name),
                command.SrcColumns,
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.DestTable)),
                command.DestColumns,
                false));

            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public override void Visit(DropForeignKeyCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialectLazy.Value.QuoteForTableName(PrefixTableName(command.SrcTable)))
                .Append(_dialectLazy.Value.GetDropForeignKeyConstraintString(PrefixTableName(command.Name)));
            _sqlStatements.Add(builder.ToString());

            RunPendingStatements();
        }

        public static string GetTypeName(Dialect dialect, DbType dbType, int? length, byte precision, byte scale) {
            return precision > 0
                       ? dialect.GetTypeName(new SqlType(dbType, precision, scale))
                       : length.HasValue
                             ? dialect.GetTypeName(new SqlType(dbType, length.Value))
                             : dialect.GetTypeName(new SqlType(dbType));
        }

        private void Visit(StringBuilder builder, CreateColumnCommand command) {
            if (ExecuteCustomInterpreter(command)) {
                return;
            }

            // name
            builder.Append(_dialectLazy.Value.QuoteForColumnName(command.ColumnName)).Append(Space);

            if (!command.IsIdentity || _dialectLazy.Value.HasDataTypeInIdentityColumn) {
                builder.Append(GetTypeName(_dialectLazy.Value, command.DbType, command.Length, command.Precision, command.Scale));
            }

            // append identity if handled
            if (command.IsIdentity && _dialectLazy.Value.SupportsIdentityColumns) {
                builder.Append(Space).Append(_dialectLazy.Value.IdentityColumnString);
            }

            // [default value]
            if (command.Default != null) {
                builder.Append(" default ").Append(ConvertToSqlValue(command.Default)).Append(Space);
            }

            // nullable
            builder.Append(command.IsNotNull
                               ? " not null"
                               : !command.IsPrimaryKey && !command.IsUnique
                                     ? _dialectLazy.Value.NullColumnString
                                     : string.Empty);

            // append unique if handled, otherwise at the end of the satement
            if (command.IsUnique && _dialectLazy.Value.SupportsUnique) {
                builder.Append(" unique");
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Nothing comes from user input.")]
        private void RunPendingStatements() {

            var session = _transactionManager.GetSession();

            try {
                foreach (var sqlStatement in _sqlStatements) {
                    Logger.Debug(sqlStatement);

                    using (var command = session.Connection.CreateCommand()) {
                        command.CommandText = sqlStatement;
                        session.Transaction.Enlist(command);
                        command.ExecuteNonQuery();
                    }
                 
                    _reportsCoordinator.Information("Data Migration", String.Format("Executing SQL Query: {0}", sqlStatement));
                }
            }
            finally {
                _sqlStatements.Clear();    
            }
        }

        private bool ExecuteCustomInterpreter<T>(T command) where T : ISchemaBuilderCommand {
            var interpreter = _commandInterpreters
                .Where(ici => ici.DataProvider == _shellSettings.DataProvider)
                .OfType<ICommandInterpreter<T>>()
                .FirstOrDefault();

            if (interpreter != null) {
                _sqlStatements.AddRange(interpreter.CreateStatements(command));
                RunPendingStatements();
                return true;
            }

            return false;
        }

        public static string ConvertToSqlValue(object value) {
            if ( value == null ) {
                return "null";
            }
            
            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.String:
                case TypeCode.Char:
                    return String.Concat("'", Convert.ToString(value).Replace("'", "''"), "'");
                case TypeCode.Boolean:
                    return (bool) value ? "1" : "0";
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                case TypeCode.DateTime:
                    return String.Concat("'", Convert.ToString(value, CultureInfo.InvariantCulture), "'");
            }

            return "null";
        }
    }
}
