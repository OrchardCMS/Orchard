using System;
using NHibernate.Dialect;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;

namespace Orchard.Data.Migration.Interpreters {
    public class SqlCeCommandInterpreter : ICommandInterpreter<DropIndexCommand> {
        private readonly Dialect _dialect;
        private readonly ShellSettings _shellSettings;

        public string DataProvider {
            get { return "SqlCe"; }
        }

        public SqlCeCommandInterpreter(
            ShellSettings shellSettings,
            ISessionFactoryHolder sessionFactoryHolder) {
                _shellSettings = shellSettings;
                var configuration = sessionFactoryHolder.GetConfiguration();
                _dialect = Dialect.GetDialect(configuration.Properties);
        }

        public string[] CreateStatements(DropIndexCommand command) {
            
            return new [] { String.Format("drop index {0}.{1}",
                _dialect.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialect.QuoteForColumnName(PrefixTableName(command.IndexName)))
            };
        }

        private string PrefixTableName(string tableName) {
            if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
                return tableName;
            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
    }
}
