using System;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Messaging.Events;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.UI.PageClass;
using Orchard.Users.Handlers;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Services;
using Orchard.Tests.Messaging;
using Orchard.Tests.Modules.Stubs;
using Orchard.Tests.ContentManagement;

namespace Orchard.Tests.Modules.Users.Services {
    [TestFixture]
    public class UserServiceTests {
        private IMembershipService _membershipService;
        private IUserService _userService;
        private IClock _clock;
        private MessagingChannelStub _channel;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private IContainer _container;
        private CultureInfo _currentCulture;

        [TestFixtureSetUp]
        public void InitFixture() {
            _currentCulture = Thread.CurrentThread.CurrentCulture;
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(UserPartRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord));
        }

        [TestFixtureTearDown]
        public void TermFixture() {
            Thread.CurrentThread.CurrentCulture = _currentCulture;
        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _channel = new MessagingChannelStub();

            builder.RegisterType<MembershipService>().As<IMembershipService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterInstance(_clock = new StubClock()).As<IClock>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType(typeof(SettingsFormatter)).As<ISettingsFormatter>();
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<UserPartHandler>().As<IContentHandler>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterAutoMocking(MockBehavior.Loose);
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterInstance(new MessageChannelSelectorStub(_channel)).As<IMessageChannelSelector>(); 
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterInstance(new Mock<IPageClassBuilder>().Object);
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
            builder.RegisterType<InfosetHandler>().As<IContentHandler>();

            builder.RegisterType<DefaultEncryptionService>().As<IEncryptionService>();
            builder.RegisterInstance(ShellSettingsUtility.CreateEncryptionEnabled());

            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestTransactionManager(_session)).As<ITransactionManager>();

            _container = builder.Build();
            _membershipService = _container.Resolve<IMembershipService>();
            _userService = _container.Resolve<IUserService>();
        }

        [TearDown]
        public void TearDown() {
            if (_container != null)
                _container.Dispose();
        }

        [Test]
        public void NonceShouldBeDecryptable() {
            var user = _membershipService.CreateUser(new CreateUserParams("foo", "66554321", "foo@bar.com", "", "", true));
            var nonce = _userService.CreateNonce(user, new TimeSpan(1, 0, 0));

            Assert.That(nonce, Is.Not.Empty);

            string username;
            DateTime validateByUtc;

            var result = _userService.DecryptNonce(nonce, out username, out validateByUtc);

            Assert.That(result, Is.True);
            Assert.That(username, Is.EqualTo("foo"));
            Assert.That(validateByUtc, Is.GreaterThan(_clock.UtcNow));
        }

        [Test]
        public void VerifyUserUnicityTurkishTest() {
            CultureInfo turkishCulture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = turkishCulture;

            // Create user lower case
            _membershipService.CreateUser(new CreateUserParams("admin", "66554321", "foo@bar.com", "", "", true));

            // Verify unicity with upper case which with turkish coallition would yeld admin with an i without the dot and therefore generate a different user name
            Assert.That(_userService.VerifyUserUnicity("ADMIN", "differentfoo@bar.com"), Is.False);
        }
    }
}
