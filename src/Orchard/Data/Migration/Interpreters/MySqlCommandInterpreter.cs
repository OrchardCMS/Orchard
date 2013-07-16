using System.Data;
using System.Text;
using NHibernate.Dialect;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Localization;

namespace Orchard.Data.Migration.Interpreters {
    public class MySqlCommandInterpreter : ICommandInterpreter<AlterColumnCommand> {
        private readonly Dialect _dialect;
        private readonly ShellSettings _shellSettings;

        public MySqlCommandInterpreter() {
            T = NullLocalizer.Instance;
        }

        public string DataProvider {
            get { return "MySql"; }
        }

        public Localizer T { get; set; }

        public MySqlCommandInterpreter(
            ShellSettings shellSettings,
            ISessionFactoryHolder sessionFactoryHolder) {
                _shellSettings = shellSettings;
                var configuration = sessionFactoryHolder.GetConfiguration();
                _dialect = Dialect.GetDialect(configuration.Properties);
        }

        public string[] CreateStatements(AlterColumnCommand command) {
            var builder = new StringBuilder();
            
            builder.AppendFormat("alter table {0} modify column {1} ",
                            _dialect.QuoteForTableName(PrefixTableName(command.TableName)),
                            _dialect.QuoteForColumnName(command.ColumnName));

            // type
            if (command.DbType != DbType.Object) {
                builder.Append(DefaultDataMigrationInterpreter.GetTypeName(_dialect, command.DbType, command.Length, command.Precision, command.Scale));
            }
            else {
                if (command.Length > 0 || command.Precision > 0 || command.Scale > 0) {
                    throw new OrchardException(T("Error while executing data migration: you need to specify the field's type in order to change its properties"));
                }
            }

            // [default value]
            if (command.Default != null) {
                builder.Append(" set default ").Append(DefaultDataMigrationInterpreter.ConvertToSqlValue(command.Default)).Append(" ");
            }

            return new [] {
                builder.ToString()
            };
        }

        private string PrefixTableName(string tableName) {
            if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
                return tableName;
            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
    }
}
