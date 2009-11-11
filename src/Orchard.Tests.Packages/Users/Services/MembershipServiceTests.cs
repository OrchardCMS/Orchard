using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Modules;
using NHibernate;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Tests.Packages.Users.Services {
    [TestFixture]
    public class MembershipServiceTests {
        private IMembershipService _membershipService;
        private ISessionFactory _sessionFactory;
        private ISession _session;


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
                typeof(ModelRecord),
                typeof(ModelTypeRecord));
        }

        [TestFixtureTearDown]
        public void TermFixture() {

        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<MembershipService>().As<IMembershipService>();
            builder.Register<DefaultModelManager>().As<IModelManager>();
            builder.Register<UserDriver>().As<IModelDriver>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.Register(new TestSessionLocator(_session)).As<ISessionLocator>();
            var container = builder.Build();
            _membershipService = container.Resolve<IMembershipService>();
        }

        [Test]
        public void CreateUserShouldAllocateModelAndCreateRecords() {
            var user = _membershipService.CreateUser(new CreateUserParams("a", "b", "c", null, null, true));
            Assert.That(user.UserName, Is.EqualTo("a"));
            Assert.That(user.Email, Is.EqualTo("c"));
        }
    }
}
