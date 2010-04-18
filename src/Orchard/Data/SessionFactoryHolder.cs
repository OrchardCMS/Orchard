using System;
using System.IO;
using NHibernate;
using Orchard.Data.Builders;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Orchard.Data {
    public interface ISessionFactoryHolder : ISingletonDependency {
        ISessionFactory GetSessionFactory();
        void CreateDatabase();
        void UpdateSchema();
    }

    public class SessionFactoryHolder : ISessionFactoryHolder {
        private readonly ShellSettings _shellSettings;
        private readonly ICompositionStrategy_Obsolete _compositionStrategy;
        private readonly ISessionFactoryBuilder _sessionFactoryBuilder;
        private readonly IAppDataFolder _appDataFolder;

        private ISessionFactory _sessionFactory;

        public SessionFactoryHolder(
            ShellSettings shellSettings,
            ICompositionStrategy_Obsolete compositionStrategy,
            ISessionFactoryBuilder sessionFactoryBuilder,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _compositionStrategy = compositionStrategy;
            _sessionFactoryBuilder = sessionFactoryBuilder;
            _appDataFolder = appDataFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void CreateDatabase() {
            lock (this) {
                if (_sessionFactory != null) {
                    Logger.Error("CreateSchema can not be called after a session factory was created");
                    throw new OrchardException("CreateSchema can not be called after a session factory was created");
                }

                _sessionFactory = BuildSessionFactory(true /*createDatabase*/, false /*updateSchema*/);
            }
        }

        public void UpdateSchema() {
            lock (this) {
                if (_sessionFactory != null) {
                    Logger.Error("UpdateSchema can not be called after a session factory was created");
                    throw new OrchardException("UpdateSchema can not be called after a session factory was created");
                }

                _sessionFactory = BuildSessionFactory(false /*createDatabase*/, true /*updateSchema*/);
            }
        }

        public ISessionFactory GetSessionFactory() {
            lock (this) {
                if (_sessionFactory == null) {
                    _sessionFactory = BuildSessionFactory(false /*createDatabase*/, true /*updateSchema*/);
                }
            }
            return _sessionFactory;
        }

        private ISessionFactory BuildSessionFactory(bool createDatabase, bool updateSchema) {
            Logger.Debug("Building session factory");

            var shellPath = _appDataFolder.CreateDirectory(Path.Combine("Sites", _shellSettings.Name));

            var sessionFactory = _sessionFactoryBuilder.BuildSessionFactory(new SessionFactoryParameters {
                Provider = _shellSettings.DataProvider,
                DataFolder = shellPath,
                ConnectionString = _shellSettings.DataConnectionString,
                CreateDatabase = createDatabase,
                UpdateSchema = updateSchema,
                RecordDescriptors = _compositionStrategy.GetRecordDescriptors_Obsolete(),
            });

            return sessionFactory;
        }

    }


}
