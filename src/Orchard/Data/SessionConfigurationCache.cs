using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Logging;
using Orchard.Utility;

namespace Orchard.Data {
    public class SessionConfigurationCache : ISessionConfigurationCache {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IAppDataFolder _appDataFolder;

        public SessionConfigurationCache(ShellSettings shellSettings, ShellBlueprint shellBlueprint, IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _appDataFolder = appDataFolder;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public Configuration GetConfiguration(Func<Configuration> builder) {
            var hash = ComputeHash(_shellBlueprint).Value;

            // Return previous configuration if it exsists and has the same hash as
            // the current blueprint.
            var previousConfig = ReadConfiguration(_shellSettings.Name);
            if (previousConfig != null) {
                if (previousConfig.ShellName == _shellSettings.Name && previousConfig.Hash == hash) {
                    return previousConfig.Configuration;
                }
            }

            // Create cache and persist it
            var cache = new ConfigurationCache {
                ShellName = _shellSettings.Name,
                Hash = hash,
                Configuration = builder()
            };

            StoreConfiguration(cache);
            return cache.Configuration;
        }

        [Serializable]
        public class ConfigurationCache {
            public string ShellName { get; set; }
            public string Hash { get; set; }
            public Configuration Configuration { get; set; }
        }

        private void StoreConfiguration(ConfigurationCache cache) {
            var pathName = GetPathName(cache.ShellName);

            using (var stream = _appDataFolder.CreateFile(pathName)) {
                new BinaryFormatter().Serialize(stream, cache);
            }
        }

        private ConfigurationCache ReadConfiguration(string shellName) {
            var pathName = GetPathName(shellName);

            if (!_appDataFolder.FileExists(pathName))
                return null;

            try {
                using (var stream = _appDataFolder.OpenFile(pathName)) {
                    return new BinaryFormatter().Deserialize(stream) as ConfigurationCache;
                }
            }
            catch (Exception e) {
                for (var scan = e; scan != null; scan = scan.InnerException)
                    Logger.Warning("The cached NHibernate configuration cache can't be read: {0}", scan.Message);
                return null;
            }
        }

        private Hash ComputeHash(ShellBlueprint shellBlueprint) {
            // We need to hash the assemnly names, record names and property names
            var hash = new Hash();

            foreach (var tableName in shellBlueprint.Records.Select(x => x.TableName)) {
                hash.AddString(tableName);
            }

            foreach (var recordType in shellBlueprint.Records.Select(x => x.Type)) {
                hash.AddTypeReference(recordType);

                foreach (var property in recordType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public)) {
                    hash.AddString(property.Name);
                    hash.AddTypeReference(property.PropertyType);
                }
            }

            return hash;
        }

        private string GetPathName(string shellName) {
            return _appDataFolder.Combine("Sites", shellName, "mappings.bin");
        }
    }
}
