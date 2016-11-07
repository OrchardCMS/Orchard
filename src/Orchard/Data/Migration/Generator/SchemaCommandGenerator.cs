using System;
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
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;

namespace Orchard.Data.Migration.Generator {
    public class SchemaCommandGenerator : ISchemaCommandGenerator {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IExtensionManager _extensionManager;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly ShellSettings _shellSettings;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;

        public SchemaCommandGenerator(
            ISessionFactoryHolder sessionFactoryHolder,
            IExtensionManager extensionManager,
            ICompositionStrategy compositionStrategy,
            ShellSettings shellSettings,
            IDataServicesProviderFactory dataServicesProviderFactory) {
            _sessionFactoryHolder = sessionFactoryHolder;
            _extensionManager = extensionManager;
            _compositionStrategy = compositionStrategy;
            _shellSettings = shellSettings;
            _dataServicesProviderFactory = dataServicesProviderFactory;
        }

        /// <summary>
        /// Generates SchemaCommand instances in order to create the schema for a specific feature
        /// </summary>
        public IEnumerable<SchemaCommand> GetCreateFeatureCommands(string feature, bool drop) {
            var dependencies = _extensionManager.AvailableFeatures()
                .Where(f => String.Equals(f.Id, feature, StringComparison.OrdinalIgnoreCase))
                .Where(f => f.Dependencies != null)
                .SelectMany(f => f.Dependencies)
                .ToList();

            var shellDescriptor = new ShellDescriptor {
                Features = dependencies.Select(id => new ShellFeature { Name = id }).Union(new[] { new ShellFeature { Name = feature }, new ShellFeature { Name = "Orchard.Framework" } })
            };

            var shellBlueprint = _compositionStrategy.Compose(_shellSettings, shellDescriptor);

            if ( !shellBlueprint.Records.Any() ) {
                yield break;
            }

            //var features = dependencies.Select(name => new ShellFeature {Name = name}).Union(new[] {new ShellFeature {Name = feature}, new ShellFeature {Name = "Orchard.Framework"}});

            var parameters = _sessionFactoryHolder.GetSessionFactoryParameters();
            parameters.RecordDescriptors = shellBlueprint.Records.ToList();

            var configuration = _dataServicesProviderFactory
                .CreateProvider(parameters)
                .BuildConfiguration(parameters);

            Dialect.GetDialect(configuration.Properties);
            var mapping = configuration.BuildMapping();

            // get the table mappings using reflection
            var tablesField = typeof(Configuration).GetField("tables", BindingFlags.Instance | BindingFlags.NonPublic);
            var tables = ((IDictionary<string, Table>) tablesField.GetValue(configuration)).Values;

            string prefix = feature.Replace(".", "_") + "_";

            foreach(var table in tables.Where(t => parameters.RecordDescriptors.Any(rd => rd.Feature.Descriptor.Id == feature && rd.TableName == t.Name))) {
                string tableName = table.Name;
                var recordType = parameters.RecordDescriptors.First(rd => rd.Feature.Descriptor.Id == feature && rd.TableName == tableName).Type;
                var isContentPart = typeof(ContentPartRecord).IsAssignableFrom(recordType);

                if ( tableName.StartsWith(prefix) ) {
                    tableName = tableName.Substring(prefix.Length);
                }

                if(drop) {
                    yield return new DropTableCommand(tableName);
                }

                var command = new CreateTableCommand(tableName);
                
                foreach (var column in table.ColumnIterator) {
                    // create copies for local variables to be evaluated at the time the loop is called, and not lately when the la;bda is executed
                    var tableCopy = table;
                    var columnCopy = column;

                    var sqlType = columnCopy.GetSqlTypeCode(mapping);
                    command.Column(column.Name, sqlType.DbType,
                        action => {
                            if (tableCopy.PrimaryKey.Columns.Any(c => c.Name == columnCopy.Name)) {
                                action.PrimaryKey();

                                if ( !isContentPart ) {
                                    action.Identity();
                                }
                            }

                            
                            if ( columnCopy.IsLengthDefined() 
                                && new[] { DbType.StringFixedLength, DbType.String, DbType.AnsiString, DbType.AnsiStringFixedLength }.Contains(sqlType.DbType)
                                && columnCopy.Length != Column.DefaultLength) {
                                action.WithLength(columnCopy.Length);
                            }

                            if (columnCopy.IsPrecisionDefined()) {
                                action.WithPrecision((byte) columnCopy.Precision);
                                action.WithScale((byte) columnCopy.Scale);
                            }
                            if (columnCopy.IsNullable) {
                                action.Nullable();
                            }

                            if ( columnCopy.IsUnique ) {
                                action.Unique();
                            }

                            if(columnCopy.DefaultValue != null) {
                                action.WithDefault(columnCopy.DefaultValue);
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
