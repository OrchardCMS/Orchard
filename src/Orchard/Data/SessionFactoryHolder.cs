using NHibernate;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Data {
    public interface ISessionFactoryHolder : ISingletonDependency {
        ISessionFactory GetSessionFactory();
        SessionFactoryParameters GetSessionFactoryParameters();
    }

    public class SessionFactoryHolder : ISessionFactoryHolder {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IAppDataFolder _appDataFolder;

        private ISessionFactory _sessionFactory;

        public SessionFactoryHolder(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IDataServicesProviderFactory dataServicesProviderFactory,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _appDataFolder = appDataFolder;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ISessionFactory GetSessionFactory() {
            lock (this) {
                if (_sessionFactory == null) {
                    _sessionFactory = BuildSessionFactory();
                }
            }
            return _sessionFactory;
        }

        private ISessionFactory BuildSessionFactory() {
            Logger.Debug("Building session factory");

            var parameters = GetSessionFactoryParameters();

            var sessionFactory = _dataServicesProviderFactory
                .CreateProvider(parameters)
                .BuildConfiguration(parameters)
                .BuildSessionFactory();

            return sessionFactory;
        }

        public SessionFactoryParameters GetSessionFactoryParameters() {
            var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
            _appDataFolder.CreateDirectory(shellPath);

            var shellFolder = _appDataFolder.MapPath(shellPath);

            return new SessionFactoryParameters {
                Provider = _shellSettings.DataProvider,
                DataFolder = shellFolder,
                ConnectionString = _shellSettings.DataConnectionString,
                RecordDescriptors = _shellBlueprint.Records,
            };
        }
    }


}
