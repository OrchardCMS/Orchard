using System.IO;
using Autofac;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Tests.ContentManagement;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Localization {
    [TestFixture]
    public class CurrentCultureWorkContextTests {
        private IContainer _container;
        private IWorkContextStateProvider _currentCultureStateProvider;
        private WorkContext _workContext;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private string _databaseFileName;
        private const string _testCulture = "fr-CA";

        [TestFixtureSetUp]
        public void InitFixture() {
            _databaseFileName = Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                _databaseFileName,
                typeof(CultureRecord));
        }

        [SetUp]
        public void Init() {
            _session = _sessionFactory.OpenSession();

            var builder = new ContainerBuilder();
            _workContext = new StubWorkContext();
            builder.RegisterInstance(new StubCultureSelector(_testCulture)).As<ICultureSelector>();
            builder.RegisterInstance(new StubHttpContext("~/"));
            builder.RegisterInstance(_workContext);
            builder.RegisterType<StubHttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<CurrentCultureWorkContext>().As<IWorkContextStateProvider>();
            builder.RegisterType<DefaultCultureManager>().As<ICultureManager>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType<Signals>().As<ISignals>().SingleInstance();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestTransactionManager(_session)).As<ITransactionManager>();
            _container = builder.Build();
            _currentCultureStateProvider = _container.Resolve<IWorkContextStateProvider>();
            _container.Resolve<ICultureManager>().AddCulture(_testCulture);
        }

        [TearDown]
        public void Term() {
            _session.Close();
        }

        [TestFixtureTearDown]
        public void TermFixture() {
            File.Delete(_databaseFileName);
        }

        [Test]
        public void CultureManagerReturnsCultureFromSelectors() {
            var actualCulture = _currentCultureStateProvider.Get<string>("CurrentCulture")(_workContext);
            Assert.That(actualCulture, Is.EqualTo(_testCulture));
        }
    }
}