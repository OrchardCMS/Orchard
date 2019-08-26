using NHibernate.Dialect;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Orchard.Data.Migration.Interpreters {
    public class PostgreSqlCommandInterpreter : ICommandInterpreter<AlterColumnCommand> {
        private readonly Lazy<Dialect> _dialectLazy;
        private readonly ShellSettings _shellSettings;
        private readonly DefaultDataMigrationInterpreter _dataMigrationInterpreter;

        public PostgreSqlCommandInterpreter(DefaultDataMigrationInterpreter dataMigrationInterpreter) {
            _dataMigrationInterpreter = dataMigrationInterpreter;
            T = NullLocalizer.Instance;
        }

        public string DataProvider {
            get { return "PostgreSql"; }
        }

        public Localizer T { get; set; }

        public PostgreSqlCommandInterpreter(
            ShellSettings shellSettings,
            ISessionFactoryHolder sessionFactoryHolder) {
            _shellSettings = shellSettings;
            _dialectLazy = new Lazy<Dialect>(() => Dialect.GetDialect(sessionFactoryHolder.GetConfiguration().Properties));
        }

        public string[] CreateStatements(AlterColumnCommand command) {
            var builder = new StringBuilder();

            builder.AppendFormat("alter table {0} alter column {1} ",
                            _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                            _dialectLazy.Value.QuoteForColumnName(command.ColumnName));
            var initLength = builder.Length;

            // type
            if (command.DbType != DbType.Object) {
                builder.AppendFormat("type {0}",
                    DefaultDataMigrationInterpreter.GetTypeName(_dialectLazy.Value, command.DbType, command.Length, command.Precision, command.Scale));
            }
            else {
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
    }
}
