using System.Linq;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using Orchard.Tests.Records;

namespace Orchard.Tests {
    [TestFixture]
    public class LinqToNHibernateTests {
        #region Setup/Teardown

        [SetUp]
        public void Init() {
            var sessionFactory = DataUtility.CreateSessionFactory(typeof (FooRecord));
            using (var session = sessionFactory.OpenSession()) {
                session.Save(new FooRecord {Name = "one"});
                session.Save(new FooRecord {Name = "two"});
                session.Save(new FooRecord {Name = "three"});
            }
            _session = sessionFactory.OpenSession();
        }

        [TearDown]
        public void Term() {
            _session.Close();
        }

        #endregion

        private ISession _session;

        [Test]
        public void WhereClauseShouldLimitResults() {
            var foos = from f in _session.Query<FooRecord>() where f.Name == "two" || f.Name == "one" select f;

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("one"));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
            Assert.That(foos, Has.None.Property("Name").EqualTo("three"));
        }
    }
}