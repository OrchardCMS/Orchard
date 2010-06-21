using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;

namespace Orchard.DataMigration {
    /// <summary>
    /// Reponsible for maintaining the knowledge of data migration in a per tenant table
    /// </summary>
    public class DataMigrationManager : IDataMigrationManager {
        private readonly IEnumerable<IDataMigration> _dataMigrations;
        private readonly IRepository<DataMigrationRecord> _dataMigrationRepository;
        private readonly IDataMigrationGenerator _dataMigrationGenerator;
        private readonly IExtensionManager _extensionManager;

        public DataMigrationManager(
            IEnumerable<IDataMigration> dataMigrations, 
            IRepository<DataMigrationRecord> dataMigrationRepository,
            IDataMigrationGenerator dataMigrationGenerator,
            IExtensionManager extensionManager) {
            _dataMigrations = dataMigrations;
            _dataMigrationRepository = dataMigrationRepository;
            _dataMigrationGenerator = dataMigrationGenerator;
            _extensionManager = extensionManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Upgrade(string feature) {

            // proceed with dependent features first, whatever the module it's in
            var dependencies = _extensionManager
                .AvailableExtensions()
                .SelectMany(e => e.Features)
                .Where(f => String.Equals(f.Name, feature, StringComparison.OrdinalIgnoreCase))
                .Where(f => f.Dependencies != null)
                .SelectMany( f => f.Dependencies )
                .ToList();

            foreach(var dependency in dependencies) {
                Upgrade(dependency);
            }

            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module
            foreach ( var migration in migrations ) {
                // copy the objet for the Linq query
                var tempMigration = migration;
                
                // get current version for this migration
                var dataMigrationRecord = _dataMigrationRepository.Table
                    .Where(dm => dm.DataMigrationClass == tempMigration.GetType().FullName)
                    .FirstOrDefault();

                var updateMethodNameRegex = new Regex(@"^UpdateFrom(?<version>\d+)$", RegexOptions.Compiled);
                var current = 0;
                if(dataMigrationRecord != null) {
                    current = dataMigrationRecord.Current;
                }

                // do we need to call Create() ?
                if(current == 0) {
                    // try to resolve a Create method

                    var createMethod = migration.GetType().GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
                    if(createMethod != null && createMethod.ReturnType == typeof(int)) {
                        current = (int)createMethod.Invoke(migration, new object[0]);
                    }
                    else {
                        var commands = _dataMigrationGenerator.CreateCommands();
                        /// TODO: Execute commands and define current version number
                    }
                }

                // update methods might also be called after Create()
                var lookupTable = new Dictionary<int, MethodInfo>();
                // construct a lookup table with all managed initial versions
                foreach(var methodInfo in migration.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                    var match = updateMethodNameRegex.Match(methodInfo.Name);
                    if(match.Success) {
                        lookupTable.Add(int.Parse(match.Groups["version"].Value), methodInfo);
                    }
                }

                while(lookupTable.ContainsKey(current)) {
                    try {
                        Logger.Information("Applying migration for {0} from version {1}", feature, current);
                        current = (int)lookupTable[current].Invoke(migration, new object[0]);
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "An unexpected error orccured while applying migration on {0} from version {1}", feature, current);
                    }
                }

                // if current is 0, it means no upgrade/create method was found or succeeded 
                if ( current != 0 ) {
                    if (dataMigrationRecord == null) {
                        _dataMigrationRepository.Create(new DataMigrationRecord {Current = current, DataMigrationClass = migration.GetType().FullName});
                    }
                    else {
                        dataMigrationRecord.Current = current;
                        _dataMigrationRepository.Update(dataMigrationRecord);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all the available IDataMigration instances for a specific module
        /// </summary>
        public IEnumerable<IDataMigration> GetDataMigrations(string feature) {
            return _dataMigrations
                    .Where(dm => String.Equals(dm.Feature, feature, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }
    }
}
