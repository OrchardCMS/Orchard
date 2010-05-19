using Autofac;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.MetaData.Records;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;


namespace Orchard.Tests.ContentManagement{
    [TestFixture]
    class ContentTypeMetaDataTests 
    {
        private IContainer _container;
        private ISessionFactory _sessionFactory;
        private ISession _session;

        [TestFixtureSetUp]
        public void InitFixture()
        {
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(ContentTypeRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypePartRecord),
                typeof(ContentTypePartNameRecord),
                typeof(ContentItemVersionRecord));
        }

        [TestFixtureTearDown]
        public void TermFixture()
        {

        }

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<ContentTypeService>().As<IContentTypeService>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();

            _container = builder.Build();
        }

        [Test]
        public void MapandUnMapContentTypeToContentPart()
        {
            var contentTypeService = _container.Resolve<IContentTypeService>();
            contentTypeService.MapContentTypeToContentPart("foo", "bar");
            Assert.IsTrue(contentTypeService.ValidateContentTypeToContentPartMapping("foo","bar"),"Content Type not successfully mapped");
            contentTypeService.UnMapContentTypeToContentPart("foo", "bar");
            Assert.IsFalse(contentTypeService.ValidateContentTypeToContentPartMapping("foo", "bar"), "Content Type mapping not successfully deleted");

        }

    }
}
