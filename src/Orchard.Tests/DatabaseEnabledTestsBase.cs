using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autofac;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Services;
using Orchard.Tests.ContentManagement;
using Orchard.Tests.Data;
using Orchard.Tests.Stubs;

namespace Orchard.Tests {
    public abstract class DatabaseEnabledTestsBase {

        protected IContainer _container;

        protected ISession _session;
        protected string _databaseFilePath;
        protected ISessionFactory _sessionFactory;
        protected StubClock _clock;


        [TestFixtureSetUp]
        public void InitFixture() {
        }

        [TestFixtureTearDown]
        public void TearDownFixture() {
            File.Delete(_databaseFilePath);
        }

        [SetUp]
        public virtual void Init() {
            _databaseFilePath = Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(_databaseFilePath, DatabaseTypes.ToArray());
            _session = _sessionFactory.OpenSession();
            _clock = new StubClock();

            var builder = new ContainerBuilder();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterType<InfosetHandler>().As<IContentHandler>();
            builder.RegisterInstance(new StubLocator(_session)).As<ISessionLocator>();
            builder.RegisterInstance(_clock).As<IClock>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterInstance(new ShellSettings { Name = ShellSettings.DefaultName, DataProvider = "SqlCe" });
            builder.RegisterInstance(new TestTransactionManager(_session)).As<ITransactionManager>();

            Register(builder);
            _container = builder.Build();
        }

        [TearDown]
        public void Cleanup() {
            if(_container != null)
                _container.Dispose();
        }

        public abstract void Register(ContainerBuilder builder);

        protected virtual IEnumerable<Type> DatabaseTypes {
            get {
                return Enumerable.Empty<Type>();
            }
        }

        protected void ClearSession() {
            Trace.WriteLine("Flush and clear session");
            _session.Flush();
            _session.Clear();
            Trace.WriteLine("Flushed and cleared session");
        }
    }
}