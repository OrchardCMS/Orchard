using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Logging;
using Orchard.Utility;
using Orchard.Exceptions;

namespace Orchard.Data {
    public class SessionConfigurationCache : ISessionConfigurationCache {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IEnumerable<ISessionConfigurationEvents> _configurers;
        private ConfigurationCache _currentConfig;

        public SessionConfigurationCache(ShellSettings shellSettings, ShellBlueprint shellBlueprint, IAppDataFolder appDataFolder, IHostEnvironment hostEnvironment, IEnumerable<ISessionConfigurationEvents> configurers) {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _appDataFolder = appDataFolder;
            _hostEnvironment = hostEnvironment;
            _configurers = configurers;
            _currentConfig = null;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        public Configuration GetConfiguration(Func<Configuration> builder) {
            if (Disabled) {
                return builder();
            }

            var hash = ComputeHash().Value;

            // if the current configuration is unchanged, return it
            if(_currentConfig != null && _currentConfig.Hash == hash) {
                return _currentConfig.Configuration;
            }

            // Return previous configuration if it exists and has the same hash as
            // the current blueprint.
            var previousConfig = ReadConfiguration(hash);
            if (previousConfig != null) {
                _currentConfig = previousConfig;
                return previousConfig.Configuration;
            }

            // Create cache and persist it
            _currentConfig = new ConfigurationCache {
                Hash = hash,
                Configuration = builder()
            };

            StoreConfiguration(_currentConfig);
            return _currentConfig.Configuration;
        }

        private class ConfigurationCache {
            public string Hash { get; set; }
            public Configuration Configuration { get; set; }
        }

        private void StoreConfiguration(ConfigurationCache cache) {
            var pathName = GetPathName(_shellSettings.Name);

            try {
                var formatter = new BinaryFormatter();
                using (var stream = _appDataFolder.CreateFile(pathName)) {
                    formatter.Serialize(stream, cache.Hash);
                    formatter.Serialize(stream, cache.Configuration);
                }
            }
            catch (SerializationException ex) {
                //Note: This can happen when multiple processes/AppDomains try to save
                //      the cached configuration at the same time. Only one concurrent
                //      writer will win, and it's harmless for the other ones to fail.
                for (Exception scan = ex; scan != null; scan = scan.InnerException)
                    Logger.Warning("Error storing new NHibernate cache configuration: {0}", scan.Message);
            }
        }

        private ConfigurationCache ReadConfiguration(string hash) {
            var pathName = GetPathName(_shellSettings.Name);

            if (!_appDataFolder.FileExists(pathName))
                return null;

            try {
                var formatter = new BinaryFormatter();
                using (var stream = _appDataFolder.OpenFile(pathName)) {

                    // if the stream is empty, stop here
                    if(stream.Length == 0) {
                        return null;
                    }

                    var oldHash = (string)formatter.Deserialize(stream);
                    if (hash != oldHash) {
                        Logger.Information("The cached NHibernate configuration is out of date. A new one will be re-generated.");
                        return null;
                    }

                    var oldConfig = (Configuration)formatter.Deserialize(stream);

                    return new ConfigurationCache {
                        Hash = oldHash,
                        Configuration = oldConfig
                    };
                }
            }
            catch (Exception ex) {
                if (ex.IsFatal()) {
                    throw;
                } 
                for (var scan = ex; scan != null; scan = scan.InnerException)
                    Logger.Warning("Error reading the cached NHibernate configuration: {0}", scan.Message);
                Logger.Information("A new one will be re-generated.");
                return null;
            }
        }

        private Hash ComputeHash() {
            var hash = new Hash();

            // Shell settings physical location
            //   The nhibernate configuration stores the physical path to the SqlCe database
            //   so we need to include the physical location as part of the hash key, so that
            //   xcopy migrations work as expected.
            var pathName = GetPathName(_shellSettings.Name);
            hash.AddString(_appDataFolder.MapPath(pathName).ToLowerInvariant());

            // Orchard version, to rebuild the mappings for each new version
            var orchardVersion = new System.Reflection.AssemblyName(typeof(Orchard.ContentManagement.ContentItem).Assembly.FullName).Version.ToString();
            hash.AddString(orchardVersion);

            // Shell settings data
            hash.AddString(_shellSettings.DataProvider);
            hash.AddString(_shellSettings.DataTablePrefix);
            hash.AddString(_shellSettings.DataConnectionString);
            hash.AddString(_shellSettings.Name);

            // Assembly names, record names and property names
            foreach (var tableName in _shellBlueprint.Records.Select(x => x.TableName)) {
                hash.AddString(tableName);
            }

            foreach (var recordType in _shellBlueprint.Records.Select(x => x.Type)) {
                hash.AddTypeReference(recordType);

                if (recordType.BaseType != null)
                    hash.AddTypeReference(recordType.BaseType);

                foreach (var property in recordType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) {
                    hash.AddString(property.Name);
                    hash.AddTypeReference(property.PropertyType);

                    foreach (var attr in property.GetCustomAttributesData()) {
                        hash.AddTypeReference(attr.Constructor.DeclaringType);
                    }
                }
            }

            _configurers.Invoke(c => c.ComputingHash(hash), Logger);

            return hash;
        }

        private string GetPathName(string shellName) {
            return _appDataFolder.Combine("Sites", shellName, "mappings.bin");
        }
    }
}
