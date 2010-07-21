using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Records;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Extensions;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Reports.Services;

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

            var features = new List<string>();

            // compare current version and available migration methods for each migration class
            foreach ( var dataMigration in _dataMigrations ) {
                
                // get current version for this migration
                var dataMigrationRecord = GetDataMigrationRecord(dataMigration);

                var current = 0;
                if (dataMigrationRecord != null) {
                    current = dataMigrationRecord.Version;
                }

                // do we need to call Create() ?
                if (current == 0) {
                    
                    // try to resolve a Create method
                    if ( GetCreateMethod(dataMigration) != null ) {
                        features.Add(dataMigration.Feature.Descriptor.Name);
                        continue;
                    }
                }

                var lookupTable = CreateUpgradeLookupTable(dataMigration);

                if(lookupTable.ContainsKey(current)) {
                    features.Add(dataMigration.Feature.Descriptor.Name);
                }
            }

            return features;
        }

        public void Update(IEnumerable<string> features) {
            foreach(var feature in features) {
                Update(feature);
            }
        }

        public void Update(string feature){

            Logger.Information("Updating feature: {0}", feature);

            // proceed with dependent features first, whatever the module it's in
            var dependencies = ShellStateCoordinator.OrderByDependencies(_extensionManager.AvailableExtensions()
                .SelectMany(ext => ext.Features))
                .Where(f => String.Equals(f.Name, feature, StringComparison.OrdinalIgnoreCase))
                .Where(f => f.Dependencies != null)
                .SelectMany( f => f.Dependencies )
                .ToList();

            foreach(var dependency in dependencies) {
                Update(dependency);
            }

            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module))))
            foreach ( var migration in migrations ) {
                // copy the objet for the Linq query
                var tempMigration = migration;
                
                // get current version for this migration
                var dataMigrationRecord = GetDataMigrationRecord(tempMigration);

                var current = 0;
                if(dataMigrationRecord != null) {
                    current = dataMigrationRecord.Version;
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
                    _dataMigrationRepository.Update(dataMigrationRecord);
                }
            }
        }

        public void Uninstall(string feature) {
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
                    .Where(dm => String.Equals(dm.Feature.Descriptor.Name, feature, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            foreach (var migration in migrations.OfType<DataMigrationImpl>()) {
                migration.SchemaBuilder = new SchemaBuilder(_interpreter, migration.Feature.Descriptor.Name.Replace(".", "_") + "_");
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
            var updateMethodNameRegex = new Regex(@"^UpdateFrom(?<version>\d+)$", RegexOptions.Compiled);

            // update methods might also be called after Create()
            var lookupTable = new Dictionary<int, MethodInfo>();

            // construct a lookup table with all managed initial versions
            foreach ( var methodInfo in dataMigration.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance) ) {
                var match = updateMethodNameRegex.Match(methodInfo.Name);
                if ( match.Success ) {
                    lookupTable.Add(int.Parse(match.Groups["version"].Value), methodInfo);
                }
            }

            return lookupTable;
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
