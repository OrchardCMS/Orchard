using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Autofac;
using Autofac.Builder;
using Autofac.Modules;
using NHibernate;
using NUnit.Framework;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Tests.Packages.Users.Services {
    [TestFixture]
    public class MembershipServiceTests {
        private IMembershipService _membershipService;
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
                typeof(UserRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord));
        }

        [TestFixtureTearDown]
        public void TermFixture() {

        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<MembershipService>().As<IMembershipService>();
            builder.Register<DefaultContentManager>().As<IContentManager>();
            builder.Register<UserHandler>().As<IContentHandler>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.Register(new TestSessionLocator(_session)).As<ISessionLocator>();
            _container = builder.Build();
            _membershipService = _container.Resolve<IMembershipService>();
        }

        [Test]
        public void CreateUserShouldAllocateModelAndCreateRecords() {
            var user = _membershipService.CreateUser(new CreateUserParams("a", "b", "c", null, null, true));
            Assert.That(user.UserName, Is.EqualTo("a"));
            Assert.That(user.Email, Is.EqualTo("c"));
        }

        [Test]
        public void DefaultPasswordFormatShouldBeHashedAndHaveSalt() {
            var user = _membershipService.CreateUser(new CreateUserParams("a", "b", "c", null, null, true));

            var userRepository = _container.Resolve<IRepository<UserRecord>>();
            var userRecord = userRepository.Get(user.Id);
            Assert.That(userRecord.PasswordFormat, Is.EqualTo(MembershipPasswordFormat.Hashed));
            Assert.That(userRecord.Password, Is.Not.EqualTo("b"));
            Assert.That(userRecord.PasswordSalt, Is.Not.Null);
            Assert.That(userRecord.PasswordSalt, Is.Not.Empty);
        }

        [Test]
        public void SaltAndPasswordShouldBeDifferentEvenWithSameSourcePassword() {
            var user1 = _membershipService.CreateUser(new CreateUserParams("a", "b", "c", null, null, true));
            _session.Flush();
            _session.Clear();

            var user2 = _membershipService.CreateUser(new CreateUserParams("d", "b", "e", null, null, true));
            _session.Flush();
            _session.Clear();

            var userRepository = _container.Resolve<IRepository<UserRecord>>();
            var user1Record = userRepository.Get(user1.Id);
            var user2Record = userRepository.Get(user2.Id);
            Assert.That(user1Record.PasswordSalt, Is.Not.EqualTo(user2Record.PasswordSalt));
            Assert.That(user1Record.Password, Is.Not.EqualTo(user2Record.Password));

            Assert.That(_membershipService.ValidateUser("a", "b"), Is.Not.Null);
            Assert.That(_membershipService.ValidateUser("d", "b"), Is.Not.Null);
        }

        [Test]
        public void ValidateUserShouldReturnNullIfUserOrPasswordIsIncorrect() {
            _membershipService.CreateUser(new CreateUserParams("test-user", "test-password", "c", null, null, true));
            _session.Flush();
            _session.Clear();

            var validate1 = _membershipService.ValidateUser("test-user", "bad-password");
            var validate2 = _membershipService.ValidateUser("bad-user", "test-password");
            var validate3 = _membershipService.ValidateUser("test-user", "test-password");

            Assert.That(validate1, Is.Null);
            Assert.That(validate2, Is.Null);
            Assert.That(validate3, Is.Not.Null);
        }
    }
}
