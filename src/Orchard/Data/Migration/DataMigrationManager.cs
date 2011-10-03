using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Records;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Data.Migration {
    /// <summary>
    /// Reponsible for maintaining the knowledge of data migration in a per tenant table
    /// </summary>
    public class DataMigrationManager : IDataMigrationManager {
        private readonly IEnumerable<IDataMigration> _dataMigrations;
        private readonly IRepository<DataMigrationRecord> _dataMigrationRepository;
        private readonly IExtensionManager _extensionManager;
        private readonly IDataMigrationInterpreter _interpreter;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DataMigrationManager(
            IEnumerable<IDataMigration> dataMigrations, 
            IRepository<DataMigrationRecord> dataMigrationRepository,
            IExtensionManager extensionManager,
            IDataMigrationInterpreter interpreter,
            IContentDefinitionManager contentDefinitionManager) {
            _dataMigrations = dataMigrations;
            _dataMigrationRepository = dataMigrationRepository;
            _extensionManager = extensionManager;
            _interpreter = interpreter;
            _contentDefinitionManager = contentDefinitionManager;

            Logger = NullLogger.Instance;
        }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<string> GetFeaturesThatNeedUpdate() {
            var currentVersions = _dataMigrationRepository.Table.ToDictionary(r => r.DataMigrationClass);

            var outOfDateMigrations = _dataMigrations.Where(dataMigration => {
                DataMigrationRecord record;
                if (currentVersions.TryGetValue(dataMigration.GetType().FullName, out record))
                    return CreateUpgradeLookupTable(dataMigration).ContainsKey(record.Version.Value);

                return (GetCreateMethod(dataMigration) != null);
            });

            return outOfDateMigrations.Select(m => m.Feature.Descriptor.Id).ToList();
        }

        public void Update(IEnumerable<string> features) {
            foreach(var feature in features) {
                Update(feature);
            }
        }

        public void Update(string feature){
            Logger.Information("Updating feature: {0}", feature);

            // proceed with dependent features first, whatever the module it's in
            var dependencies = _extensionManager.AvailableFeatures()
                .Where(f => String.Equals(f.Id, feature, StringComparison.OrdinalIgnoreCase))
                .Where(f => f.Dependencies != null)
                .SelectMany( f => f.Dependencies )
                .ToList();

            foreach(var dependency in dependencies) {
                Update(dependency);
            }

            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module
            foreach ( var migration in migrations ) {
                // copy the objet for the Linq query
                var tempMigration = migration;
                
                // get current version for this migration
                var dataMigrationRecord = GetDataMigrationRecord(tempMigration);

                var current = 0;
                if(dataMigrationRecord != null) {
                    current = dataMigrationRecord.Version.Value;
                }

                // do we need to call Create() ?
                if(current == 0) {
                    // try to resolve a Create method

                    var createMethod = GetCreateMethod(migration);
                    if(createMethod != null) {
                        current = (int)createMethod.Invoke(migration, new object[0]);
                    }
                }

                var lookupTable = CreateUpgradeLookupTable(migration);

                while(lookupTable.ContainsKey(current)) {
                    try {
                        Logger.Information("Applying migration for {0} from version {1}", feature, current);
                        current = (int)lookupTable[current].Invoke(migration, new object[0]);
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "An unexpected error orccured while applying migration on {0} from version {1}", feature, current);
                        throw;
                    }
                }

                // if current is 0, it means no upgrade/create method was found or succeeded 
                if (current == 0) {
                    continue;
                }
                if (dataMigrationRecord == null) {
                    _dataMigrationRepository.Create(new DataMigrationRecord {Version = current, DataMigrationClass = migration.GetType().FullName});
                }
                else {
                    dataMigrationRecord.Version = current;
                }
            }
        }

        public void Uninstall(string feature) {
            Logger.Information("Uninstalling feature: {0}", feature);

            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module
            foreach (var migration in migrations) {
                // copy the object for the Linq query
                var tempMigration = migration;

                // get current version for this migration
                var dataMigrationRecord = GetDataMigrationRecord(tempMigration);

                var uninstallMethod = GetUninstallMethod(migration);
                if (uninstallMethod != null) {
                    uninstallMethod.Invoke(migration, new object[0]);
                }

                if ( dataMigrationRecord == null ) {
                    continue;
                }

                _dataMigrationRepository.Delete(dataMigrationRecord);
                _dataMigrationRepository.Flush();
            }

        }

        private DataMigrationRecord GetDataMigrationRecord(IDataMigration tempMigration) {
            return _dataMigrationRepository.Table
                .Where(dm => dm.DataMigrationClass == tempMigration.GetType().FullName)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns all the available IDataMigration instances for a specific module, and inject necessary builders
        /// </summary>
        private IEnumerable<IDataMigration> GetDataMigrations(string feature) {
            var migrations = _dataMigrations
                    .Where(dm => String.Equals(dm.Feature.Descriptor.Id, feature, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            foreach (var migration in migrations.OfType<DataMigrationImpl>()) {
                migration.SchemaBuilder = new SchemaBuilder(_interpreter, migration.Feature.Descriptor.Extension.Id, s => s.Replace(".", "_") + "_");
                migration.ContentDefinitionManager = _contentDefinitionManager;
            }

            return migrations;
        }

        /// <summary>
        /// Whether a feature has already been installed, i.e. one of its Data Migration class has already been processed
        /// </summary>
        public bool IsFeatureAlreadyInstalled(string feature) {
            return GetDataMigrations(feature).Any(dataMigration => GetDataMigrationRecord(dataMigration) != null);
        }

        /// <summary>
        /// Create a list of all available Update methods from a data migration class, indexed by the version number
        /// </summary>
        private static Dictionary<int, MethodInfo> CreateUpgradeLookupTable(IDataMigration dataMigration) {
            return dataMigration
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(GetUpdateMethod)
                .Where(tuple => tuple != null)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private static Tuple<int, MethodInfo> GetUpdateMethod(MethodInfo mi) {
            const string updatefromPrefix = "UpdateFrom";

            if (mi.Name.StartsWith(updatefromPrefix)) {
                var version = mi.Name.Substring(updatefromPrefix.Length);
                int versionValue;
                if (int.TryParse(version, out versionValue)) {
                    return new Tuple<int, MethodInfo>(versionValue, mi);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the Create method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetCreateMethod(IDataMigration dataMigration) {
            var methodInfo = dataMigration.GetType().GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
            if(methodInfo != null && methodInfo.ReturnType == typeof(int)) {
                return methodInfo;
            }

            return null;
        }

        /// <summary>
        /// Returns the Uninstall method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetUninstallMethod(IDataMigration dataMigration) {
            var methodInfo = dataMigration.GetType().GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Instance);
            if ( methodInfo != null && methodInfo.ReturnType == typeof(void) ) {
                return methodInfo;
            }

            return null;
        }

    }
}
