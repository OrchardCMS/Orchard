using System;
using System.IO;
using NHibernate;
using Orchard.Data.Builders;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Data {
    public interface ISessionFactoryHolder : ISingletonDependency {
        ISessionFactory GetSessionFactory();
        void CreateDatabase();
        void UpdateSchema();
    }

    public class SessionFactoryHolder : ISessionFactoryHolder {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly ISessionFactoryBuilder _sessionFactoryBuilder;
        private readonly IAppDataFolder _appDataFolder;

        private ISessionFactory _sessionFactory;

        public SessionFactoryHolder(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            ISessionFactoryBuilder sessionFactoryBuilder,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _sessionFactoryBuilder = sessionFactoryBuilder;
            _appDataFolder = appDataFolder;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void CreateDatabase() {
            lock (this) {
                if (_sessionFactory != null) {
                    Logger.Error("CreateSchema can not be called after a session factory was created");
                    throw new OrchardSystemException(T("CreateSchema can not be called after a session factory was created"));
                }

                _sessionFactory = BuildSessionFactory(true /*createDatabase*/, false /*updateSchema*/);
            }
        }

        public void UpdateSchema() {
            lock (this) {
                if (_sessionFactory != null) {
                    Logger.Error("UpdateSchema can not be called after a session factory was created");
                    throw new OrchardSystemException(T("UpdateSchema can not be called after a session factory was created"));
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

            var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
            _appDataFolder.CreateDirectory(shellPath);

            var shellFolder = _appDataFolder.MapPath(shellPath);

            var sessionFactory = _sessionFactoryBuilder.BuildSessionFactory(new SessionFactoryParameters {
                Provider = _shellSettings.DataProvider,
                DataFolder = shellFolder,
                ConnectionString = _shellSettings.DataConnectionString,
                CreateDatabase = createDatabase,
                UpdateSchema = updateSchema,
                RecordDescriptors = _shellBlueprint.Records,
            });

            return sessionFactory;
        }

    }


}
