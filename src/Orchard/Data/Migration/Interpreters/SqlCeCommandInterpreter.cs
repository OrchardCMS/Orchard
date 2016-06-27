using System;
using NHibernate.Dialect;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;

namespace Orchard.Data.Migration.Interpreters {
    public class SqlCeCommandInterpreter : ICommandInterpreter<DropIndexCommand> {
        private readonly Lazy<Dialect> _dialectLazy;
        private readonly ShellSettings _shellSettings;

        public string DataProvider {
            get { return "SqlCe"; }
        }

        public SqlCeCommandInterpreter(
            ShellSettings shellSettings,
            ISessionFactoryHolder sessionFactoryHolder) {
                _shellSettings = shellSettings;
                _dialectLazy = new Lazy<Dialect>(() => Dialect.GetDialect(sessionFactoryHolder.GetConfiguration().Properties));
        }

        public string[] CreateStatements(DropIndexCommand command) {
            
            return new [] { String.Format("drop index {0}.{1}",
                _dialectLazy.Value.QuoteForTableName(PrefixTableName(command.TableName)),
                _dialectLazy.Value.QuoteForColumnName(PrefixTableName(command.IndexName)))
            };
        }

        private string PrefixTableName(string tableName) {
            if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
                return tableName;
            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
    }
}
