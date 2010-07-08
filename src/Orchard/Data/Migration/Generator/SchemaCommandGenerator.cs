using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using NHibernate.Dialect;

namespace Orchard.Data.Migration.Generator {
    public class SchemaCommandGenerator : ISchemaCommandGenerator {
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;

        public SchemaCommandGenerator(
            IDataServicesProviderFactory dataServicesProviderFactory,
            ISessionFactoryHolder sessionFactoryHolder) {
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _sessionFactoryHolder = sessionFactoryHolder;
        }

        public IEnumerable<SchemaCommand> GetCreateFeatureCommands(string feature, bool drop) {
            var parameters = _sessionFactoryHolder.GetSessionFactoryParameters();

            if (!parameters.RecordDescriptors.Any()) {
                yield break;
            }

            var configuration = _dataServicesProviderFactory.CreateProvider(parameters).BuildConfiguration(parameters);
            Dialect.GetDialect(configuration.Properties);
            var mapping = configuration.BuildMapping();

            // get the tables using reflection
            var tablesField = typeof(Configuration).GetField("tables", BindingFlags.Instance | BindingFlags.NonPublic);
            var tables = ((IDictionary<string, Table>) tablesField.GetValue(configuration)).Values;

            string prefix = feature.Replace(".", "_") + "_";

            foreach(var table in tables.Where(t => parameters.RecordDescriptors.Any(rd => rd.Feature.Descriptor.Name == feature && rd.TableName == t.Name))) {
                string tableName = table.Name;
                if(tableName.StartsWith(prefix)) {
                    tableName = tableName.Substring(prefix.Length);
                }

                if(drop) {
                    yield return new DropTableCommand(tableName);
                }

                var command = new CreateTableCommand(tableName);
                
                foreach(var column in table.ColumnIterator) {
                    var table1 = table;
                    var column1 = column;
                    var sqlType = column1.GetSqlTypeCode(mapping);
                    command.Column(column.Name, sqlType.DbType,
                        action => {
                            if (table1.PrimaryKey.Columns.Any(c => c.Name == column1.Name)) {
                                action.PrimaryKey();
                            }

                            if (column1.IsLengthDefined()) {
                                action.WithLength(column1.Length);
                            }

                            if (column1.IsPrecisionDefined()) {
                                action.WithPrecision((byte) column1.Precision);
                                action.WithScale((byte) column1.Scale);
                            }
                            if (column1.IsNullable) {
                                action.Nullable();
                            }

                            if ( column1.IsUnique ) {
                                action.Unique();
                            }

                            if(column1.DefaultValue != null) {
                                action.WithDefault(column1.DefaultValue);
                            }
                        });
                }

                yield return command;
            }
        }

        public void UpdateDatabase() {
            var parameters = _sessionFactoryHolder.GetSessionFactoryParameters();
            var configuration = _dataServicesProviderFactory.CreateProvider(parameters).BuildConfiguration(parameters);
            new SchemaUpdate(configuration).Execute(false, true);
        }

        public void CreateDatabase() {
            var parameters = _sessionFactoryHolder.GetSessionFactoryParameters();
            var configuration = _dataServicesProviderFactory.CreateProvider(parameters).BuildConfiguration(parameters);
            new SchemaExport(configuration).Execute(false, true, false);
        }

    }
}
