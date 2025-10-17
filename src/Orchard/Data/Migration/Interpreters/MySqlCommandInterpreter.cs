using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NHibernate.Dialect;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Localization;

namespace Orchard.Data.Migration.Interpreters {
    public class MySqlCommandInterpreter : ICommandInterpreter<AlterColumnCommand>, ICommandInterpreter<AddIndexCommand> {
        private readonly Lazy<Dialect> _dialectLazy;
        private readonly ShellSettings _shellSettings;
        private readonly ITransactionManager _transactionManager;
        private readonly DefaultDataMigrationInterpreter _dataMigrationInterpreter;

        public MySqlCommandInterpreter(DefaultDataMigrationInterpreter dataMigrationInterpreter, ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
            _dataMigrationInterpreter = dataMigrationInterpreter;
            T = NullLocalizer.Instance;
        }

        public string DataProvider => "MySql";

        public Localizer T { get; set; }

        public MySqlCommandInterpreter(
            ShellSettings shellSettings,
            ISessionFactoryHolder sessionFactoryHolder,
            ITransactionManager transactionManager) {
            _shellSettings = shellSettings;
            _transactionManager = transactionManager;
            _dialectLazy = new Lazy<Dialect>(() => Dialect.GetDialect(sessionFactoryHolder.GetConfiguration().Properties));
        }

        public string[] CreateStatements(AlterColumnCommand command) {
            var builder = new StringBuilder();

            builder.AppendFormat("alter table {0} modify column {1} ",
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialectLazy.Value.QuoteForColumnName(command.ColumnName));
            var initLength = builder.Length;

            // Type.
            if (command.DbType != DbType.Object) {
                builder.Append(DefaultDataMigrationInterpreter.GetTypeName(
                    _dialectLazy.Value,
                    command.DbType,
                    command.Length,
                    command.Precision,
                    command.Scale));
            }
            else if (command.Length > 0 || command.Precision > 0 || command.Scale > 0) {
                throw new OrchardException(
                    T("Error while executing data migration: You need to specify the field's type in order to change its properties."));
            }

            // Default value.
            var builder2 = new StringBuilder();

            builder2.AppendFormat("alter table {0} alter column {1} ",
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialectLazy.Value.QuoteForColumnName(command.ColumnName));
            var initLength2 = builder2.Length;

            if (command.Default != null) {
                builder2.Append(" set default ").Append(_dataMigrationInterpreter.ConvertToSqlValue(command.Default)).Append(" ");
            }

            // Result.
            var result = new List<string>();

            if (builder.Length > initLength) {
                result.Add(builder.ToString());
            }

            if (builder2.Length > initLength2) {
                result.Add(builder2.ToString());
            }

            return result.ToArray();
        }

        private string PrefixTableName(string tableName) =>
            string.IsNullOrEmpty(_shellSettings.DataTablePrefix) ? tableName : $"{_shellSettings.DataTablePrefix}_{tableName}";

        public string[] CreateStatements(AddIndexCommand command) {
            var session = _transactionManager.GetSession();

            using (var sqlCommand = session.Connection.CreateCommand()) {
                var columnNames = String.Join(", ", command.ColumnNames.Select(column => $"'{column}'"));
                var tableName = PrefixTableName(command.TableName);
                var columnList = command.ColumnNames.ToList();
                var indexMaximumLength = 767;
                var longColumnNames = new List<string>();

                if (columnList.Count > 1) {
                    sqlCommand.CommandText = $@"
                        SELECT SUM(CHARACTER_MAXIMUM_LENGTH)
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE table_name = '{tableName}'
                            AND COLUMN_NAME in ({columnNames})
                            AND TABLE_SCHEMA = '{session.Connection.Database}'
                            AND (Data_type = 'varchar');";

                    using (var reader = sqlCommand.ExecuteReader()) {
                        reader.Read();
                        if (!reader.IsDBNull(0)) {
                            var characterMaximumLength = reader.GetInt32(0);
                            indexMaximumLength -= characterMaximumLength;
                            if (indexMaximumLength < 0) {
                                throw new InvalidOperationException("Cannot create index because indexMaximumLength is less than 0!");
                            }
                        }
                    }
                }
                // Check whether the index contains big nvarchar columns or text fields.
                sqlCommand.CommandText = $@"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE table_name = '{tableName}'
                        AND COLUMN_NAME in ({columnNames})
                        AND TABLE_SCHEMA = '{session.Connection.Database}'
                        AND ((Data_type = 'varchar' and CHARACTER_MAXIMUM_LENGTH > {indexMaximumLength}) OR data_type= 'text');";

                using (var reader = sqlCommand.ExecuteReader()) {
                    // Provide prefix for string columns with length more than 767.
                    while (reader.Read()) {
                        longColumnNames.Add(reader.GetString(0));
                    }

                }

                if (longColumnNames.Count > 0) {
                    var columnPrefixKeyPartLength = indexMaximumLength / longColumnNames.Count;
                    foreach (var columnName in longColumnNames) {
                        columnList[columnList.IndexOf(columnName)] = $"{columnName}({columnPrefixKeyPartLength})";
                    }
                }

                return new[] {
                    string.Format("create index {1} on {0} ({2}) ",
                        _dialectLazy.Value.QuoteForTableName(tableName),
                        _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.IndexName)),
                        string.Join(", ", columnList))};
            }
        }
    }
}
