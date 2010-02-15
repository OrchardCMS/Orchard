using System.IO;
using NHibernate;
using Orchard.Data.Builders;
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
        private readonly ISessionFactoryBuilder _sessionFactoryBuilder;
        private readonly IAppDataFolder _appDataFolder;

        private ISessionFactory _sessionFactory;

        public SessionFactoryHolder(
            IShellSettings shellSettings,
            ICompositionStrategy compositionStrategy,
            ISessionFactoryBuilder sessionFactoryBuilder,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _compositionStrategy = compositionStrategy;
            _sessionFactoryBuilder = sessionFactoryBuilder;
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

            var sessionFactory = _sessionFactoryBuilder.BuildSessionFactory(new SessionFactoryParameters {
                Provider = _shellSettings.DataProvider,
                DataFolder = shellPath,
                ConnectionString = _shellSettings.DataConnectionString,
                UpdateSchema = updateSchema,
                RecordDescriptors = _compositionStrategy.GetRecordDescriptors(),
            });

            return sessionFactory;
        }

    }


}
