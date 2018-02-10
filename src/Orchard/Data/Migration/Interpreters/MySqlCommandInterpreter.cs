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

        public string DataProvider {
            get { return "MySql"; }
        }

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

            // type
            if (command.DbType != DbType.Object) {
                builder.Append(DefaultDataMigrationInterpreter.GetTypeName(_dialectLazy.Value, command.DbType, command.Length, command.Precision, command.Scale));
            } else {
                if (command.Length > 0 || command.Precision > 0 || command.Scale > 0) {
                    throw new OrchardException(T("Error while executing data migration: you need to specify the field's type in order to change its properties"));
                }
            }

            // [default value]
            var builder2 = new StringBuilder();

            builder2.AppendFormat("alter table {0} alter column {1} ",
                            _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                            _dialectLazy.Value.QuoteForColumnName(command.ColumnName));
            var initLength2 = builder2.Length;

            if (command.Default != null) {
                builder2.Append(" set default ").Append(_dataMigrationInterpreter.ConvertToSqlValue(command.Default)).Append(" ");
            }

            // result
            var result = new List<string>();

            if (builder.Length > initLength) {
                result.Add(builder.ToString());
            }

            if (builder2.Length > initLength2) {
                result.Add(builder2.ToString());
            }

            return result.ToArray();
        }

        private string PrefixTableName(string tableName) {
            if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
                return tableName;
            return _shellSettings.DataTablePrefix + "_" + tableName;
        }

        public string[] CreateStatements(AddIndexCommand command) {
            var session = _transactionManager.GetSession();

            using (var sqlCommand = session.Connection.CreateCommand()) {
                var columnNames = String.Join(", ", command.ColumnNames.Select(c => string.Format("'{0}'", c)));
                var tableName = PrefixTableName(command.TableName);
                // check whether the index contains big nvarchar columns or text fields
                string sql = @"SELECT  COLUMN_NAME  FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE table_name = '{1}' AND COLUMN_NAME in  ({0}) AND TABLE_SCHEMA = '{2}' AND
                                     ((Data_type = 'varchar' and CHARACTER_MAXIMUM_LENGTH > 767) OR data_type= 'text');";

                sql = string.Format(sql, columnNames, tableName, session.Connection.Database);
                sqlCommand.CommandText = sql;

                var columnList = command.ColumnNames.ToList();
                using (var reader = sqlCommand.ExecuteReader()) {
                    // Provide prefix for string columns with length longer than 767
                    while (reader.Read()) {
                        var columnName = reader.GetString(0);
                        columnList[columnList.IndexOf(columnName)] = string.Format("{0}(767)", columnName);
                    }
                }

                return new[] {string.Format("create index {1} on {0} ({2}) ",
                            _dialectLazy.Value.QuoteForTableName(tableName),
                            _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.IndexName)),
                            String.Join(", ", columnList))};

            }
        }
    }
}