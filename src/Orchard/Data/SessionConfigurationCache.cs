using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg;
using Orchard.FileSystems.AppData;

namespace Orchard.Data {
    public class SessionConfigurationCache : ISessionConfigurationCache {
        private readonly IAppDataFolder _appDataFolder;

        public SessionConfigurationCache(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        public void StoreConfiguration(string shellName, Configuration config) {
            var pathName = GetPathName(shellName);

            using (var stream = _appDataFolder.CreateFile(pathName)) {
                new BinaryFormatter().Serialize(stream, config);
            }
        }

        public void DeleteConfiguration(string shellName) {
            var pathName = GetPathName(shellName);
            _appDataFolder.DeleteFile(pathName);
        }

        public void DeleteAll() {
            if (!_appDataFolder.DirectoryExists("Sites"))
                return;

            foreach (var shellName in _appDataFolder.ListDirectories("Sites"))
                DeleteConfiguration(shellName);
        }

        public Configuration GetConfiguration(string shellName) {
            var pathName = GetPathName(shellName);

            if (!_appDataFolder.FileExists(pathName)) {
                return null;
            }

            using (var stream = _appDataFolder.OpenFile(pathName)) {
                return new BinaryFormatter().Deserialize(stream) as Configuration;
            }
        }

        private string GetPathName(string shellName) {
            return _appDataFolder.Combine("Sites", shellName, "mappings.bin");
        }
    }
}