using System;
using System.Web.Security;
using System.Xml.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
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
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.Users.Handlers;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Services;
using Orchard.Environment.Configuration;
using Orchard.Tests.Messaging;

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


        public class TestSessionLocator : ISessionLocator {
            private readonly ISession _session;

            public TestSessionLocator(ISession session) {
                _session = session;
            }

            public ISession For(Type entityType) {
                return _session;
            }
        }

        [TestFixtureSetUp]
        public void InitFixture() {
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

        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterType<MembershipService>().As<IMembershipService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterInstance(_clock = new StubClock()).As<IClock>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType(typeof(SettingsFormatter))
                .As(typeof(IMapper<XElement, SettingsDictionary>))
                .As(typeof(IMapper<SettingsDictionary, XElement>));
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<UserPartHandler>().As<IContentHandler>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterAutoMocking(MockBehavior.Loose);
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterInstance(new Mock<IMessageEventHandler>().Object);
            builder.RegisterType<DefaultMessageManager>().As<IMessageManager>();
            builder.RegisterInstance(_channel = new MessagingChannelStub()).As<IMessagingChannel>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
            builder.RegisterInstance(new ShellSettings { Name = "Alpha", RequestUrlHost = "wiki.example.com", RequestUrlPrefix = "~/foo" });

            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestSessionLocator(_session)).As<ISessionLocator>();
            _container = builder.Build();
            _membershipService = _container.Resolve<IMembershipService>();
            _userService = _container.Resolve<IUserService>();
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
        public void NonceShouldNotBeUsedOnAnotherTenant() {
            var user = _membershipService.CreateUser(new CreateUserParams("foo", "66554321", "foo@bar.com", "", "", true));
            var nonce = _userService.CreateNonce(user, new TimeSpan(1, 0, 0));

            Assert.That(nonce, Is.Not.Empty);

            string username;
            DateTime validateByUtc;

            _container.Resolve<ShellSettings>().Name = "Beta";

            var result = _userService.DecryptNonce(nonce, out username, out validateByUtc);

            Assert.That(result, Is.False);
            Assert.That(username, Is.EqualTo("foo"));
            Assert.That(validateByUtc, Is.GreaterThan(_clock.UtcNow));
        }

    }
}
