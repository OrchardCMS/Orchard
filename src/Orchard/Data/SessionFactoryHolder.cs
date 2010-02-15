using System.IO;
using System.Web.Hosting;
using NHibernate;
using Orchard.Data.Migrations;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Orchard.Data {
    public interface ISessionFactoryHolder : ISingletonDependency {
        ISessionFactory GetSessionFactory();
        void UpdateSchema();
    }

    public class SessionFactoryHolder : ISessionFactoryHolder {
        private readonly IShellSettings _shellSettings;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IDatabaseManager _databaseManager;
        private readonly IAppDataFolder _appDataFolder;

        private ISessionFactory _sessionFactory;

        public SessionFactoryHolder(
            IShellSettings shellSettings,
            ICompositionStrategy compositionStrategy,
            IDatabaseManager databaseManager,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _compositionStrategy = compositionStrategy;
            _databaseManager = databaseManager;
            _appDataFolder = appDataFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void UpdateSchema() {
            lock (this) {
                if (_sessionFactory != null) {
                    Logger.Error("UpdateSchema can not be called after a session factory was created");
                    throw new OrchardException("UpdateSchema can not be called after a session factory was created");
                }

                _sessionFactory = BuildSessionFactory(true);                
            }
        }

        public ISessionFactory GetSessionFactory() {
            lock (this) {
                if (_sessionFactory == null) {
                    _sessionFactory = BuildSessionFactory(false);
                }
            }
            return _sessionFactory;
        }

        private ISessionFactory BuildSessionFactory(bool updateSchema) {
            Logger.Debug("Building session factory");

            var shellPath = _appDataFolder.CreateDirectory(Path.Combine("Sites", _shellSettings.Name));

            var coordinator = _databaseManager.CreateCoordinator(new DatabaseParameters {
                Provider = _shellSettings.DataProvider,
                DataFolder = shellPath,
                ConnectionString = _shellSettings.DataConnectionString
            });

            var sessionFactory = coordinator.BuildSessionFactory(new SessionFactoryBuilderParameters {
                CreateDatabase = false,
                UpdateSchema = updateSchema,
                RecordDescriptors = _compositionStrategy.GetRecordDescriptors()
            });

            return sessionFactory;
        }

    }


}
