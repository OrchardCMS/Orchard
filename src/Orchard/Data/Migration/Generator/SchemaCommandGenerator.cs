using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using Orchard.ContentManagement.Records;
using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using NHibernate.Dialect;

namespace Orchard.Data.Migration.Generator {
    public class SchemaCommandGenerator : ISchemaCommandGenerator {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;

        public SchemaCommandGenerator(
            ISessionFactoryHolder sessionFactoryHolder) {
            _sessionFactoryHolder = sessionFactoryHolder;
        }

        /// <summary>
        /// Generates SchemaCommand instances in order to create the schema for a specific feature
        /// </summary>
        public IEnumerable<SchemaCommand> GetCreateFeatureCommands(string feature, bool drop) {
            var parameters = _sessionFactoryHolder.GetSessionFactoryParameters();

            if (!parameters.RecordDescriptors.Any()) {
                yield break;
            }

            var configuration = _sessionFactoryHolder.GetConfiguration();
            Dialect.GetDialect(configuration.Properties);
            var mapping = configuration.BuildMapping();

            // get the table mappings using reflection
            var tablesField = typeof(Configuration).GetField("tables", BindingFlags.Instance | BindingFlags.NonPublic);
            var tables = ((IDictionary<string, Table>) tablesField.GetValue(configuration)).Values;

            string prefix = feature.Replace(".", "_") + "_";

            foreach(var table in tables.Where(t => parameters.RecordDescriptors.Any(rd => rd.Feature.Descriptor.Name == feature && rd.TableName == t.Name))) {
                string tableName = table.Name;
                var recordType = parameters.RecordDescriptors.Where(rd => rd.Feature.Descriptor.Name == feature && rd.TableName == tableName).First().Type;
                var isContentPart = typeof(ContentPartRecord).IsAssignableFrom(recordType);

                if ( tableName.StartsWith(prefix) ) {
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

                                if ( !isContentPart ) {
                                    action.Identity();
                                }
                            }

                            
                            if ( column1.IsLengthDefined() 
                                && new DbType[] { DbType.StringFixedLength, DbType.String, DbType.AnsiString, DbType.AnsiStringFixedLength }.Contains(sqlType.DbType) 
                                && column1.Length != 255 ) {
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

        /// <summary>
        /// Automatically updates a db to a functionning schema
        /// </summary>
        public void UpdateDatabase() {
            var configuration = _sessionFactoryHolder.GetConfiguration();
            new SchemaUpdate(configuration).Execute(false, true);
        }

        /// <summary>
        /// Automatically creates a db with a functionning schema
        /// </summary>
        public void CreateDatabase() {
            var configuration = _sessionFactoryHolder.GetConfiguration();
            new SchemaExport(configuration).Execute(false, true, false);
        }

    }
}
