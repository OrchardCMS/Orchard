using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NHibernate;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Tests.Models;

namespace Orchard.Tests.Data {
    [TestFixture]
    public class RepositoryTests {
        #region Setup/Teardown

        [TestFixtureSetUp]
        public void InitFixture() {
            _databaseFilePath = Path.GetTempFileName();
        }

        [SetUp]
        public void Init() {
            _sessionFactory = DataUtility.CreateSessionFactory(_databaseFilePath, typeof(Foo));
            _session = _sessionFactory.OpenSession();
            _fooRepos = new Repository<Foo>(new StubLocator(_session));
        }

        [TearDown]
        public void Term() {
            _session.Close();
        }

        [TestFixtureTearDown]
        public void TermFixture() {
            File.Delete(_databaseFilePath);
        }

        #endregion

        private IRepository<Foo> _fooRepos;
        private ISession _session;
        private string _databaseFilePath;
        private ISessionFactory _sessionFactory;

        private void CreateThreeFoos() {
            _fooRepos.Create(new Foo { Name = "one" });
            _fooRepos.Create(new Foo { Name = "two" });
            _fooRepos.Create(new Foo { Name = "three" });
        }

        [Test]
        public void GetByIdShouldReturnNullIfValueNotFound() {
            CreateThreeFoos();
            var nofoo = _fooRepos.Get(6655321);
            Assert.That(nofoo, Is.Null);
        }

        [Test]
        public void GetCanSelectSingleBasedOnFields() {
            CreateThreeFoos();

            var two = _fooRepos.Get(f => f.Name == "two");
            Assert.That(two.Name, Is.EqualTo("two"));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Sequence contains more than one element")]
        public void GetThatReturnsTwoOrMoreShouldThrowException() {
            CreateThreeFoos();
            _fooRepos.Get(f => f.Name == "one" || f.Name == "three");
        }

        [Test]
        public void GetWithZeroMatchesShouldReturnNull() {
            CreateThreeFoos();
            var nofoo = _fooRepos.Get(f => f.Name == "four");
            Assert.That(nofoo, Is.Null);
        }

        [Test]
        public void LinqCanBeUsedToSelectObjects() {
            CreateThreeFoos();

            var foos = from f in _fooRepos.Table
                       where f.Name == "one" || f.Name == "two"
                       select f;

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("one"));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
        }

        [Test]
        public void LinqExtensionMethodsCanAlsoBeUsedToSelectObjects() {
            CreateThreeFoos();

            var foos = _fooRepos.Table
                .Where(f => f.Name == "one" || f.Name == "two");

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("one"));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
        }

        [Test]
        public void OrderShouldControlResults() {
            CreateThreeFoos();

            var foos = _fooRepos.Fetch(
                f => f.Name == "two" || f.Name == "three",
                o => o.Asc(f => f.Name, f => f.Id));

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos.First().Name, Is.EqualTo("three"));
            Assert.That(foos.Skip(1).First().Name, Is.EqualTo("two"));
        }

        [Test]
        public void LinqOrderByCanBeUsedToControlResultsBug() {
            CreateThreeFoos();

            // If look at the "LinqOrderByCanBeUsedToControlResults", you will see this query
            // works fine is the static type of "foos" is "IEnumerable<Foo>"...
            IOrderedQueryable<Foo> foos =
                        from f in _fooRepos.Table
                        where f.Name == "two" || f.Name == "three"
                        orderby f.Name, f.Id ascending
                        select f;

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos.First().Name, Is.EqualTo("three"));

            // Looks like a bug in NHib implementation of IOrderedQueryable<T>
            Assert.Throws<AssertionException>(() => Assert.That(foos.Skip(1).First().Name, Is.EqualTo("two")));
        }

        [Test]
        public void LinqOrderByCanBeUsedToControlResults() {
            CreateThreeFoos();

            IEnumerable<Foo> foos =
                        from f in _fooRepos.Table
                        where f.Name == "two" || f.Name == "three"
                        orderby f.Name, f.Id ascending
                        select f;

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos.First().Name, Is.EqualTo("three"));
            Assert.That(foos.Skip(1).First().Name, Is.EqualTo("two"));
        }

        [Test]
        public void RangeShouldSliceResults() {
            for (var x = 0; x != 40; ++x) {
                _fooRepos.Create(new Foo { Name = x.ToString().PadLeft(8, '0') });
            }

            var foos = _fooRepos.Fetch(
                f => f.Name.StartsWith("000"),
                o => o.Asc(f => f.Name),
                10, 20);

            Assert.That(foos.Count(), Is.EqualTo(20));
            Assert.That(foos.First().Name, Is.EqualTo("00000010"));
            Assert.That(foos.Last().Name, Is.EqualTo("00000029"));
        }

        [Test]
        public void RepositoryCanCreateFetchUpdateAndDelete() {
            var foo1 = new Foo { Name = "yadda" };
            _fooRepos.Create(foo1);

            var foo2 = _fooRepos.Get(foo1.Id);
            foo2.Name = "blah";
            _fooRepos.Update(foo2);

            Assert.That(foo1, Is.SameAs(foo2));

            _fooRepos.Delete(foo2);
        }

        [Test]
        public void RepositoryFetchTakesCompoundLambdaPredicate() {
            CreateThreeFoos();

            var foos = _fooRepos.Fetch(f => f.Name == "three" || f.Name == "two");

            Assert.That(foos.Count(), Is.EqualTo(2));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("three"));
        }

        [Test]
        public void RepositoryFetchTakesSimpleLambdaPredicate() {
            CreateThreeFoos();

            var one = _fooRepos.Fetch(f => f.Name == "one").Single();
            var two = _fooRepos.Fetch(f => f.Name == "two").Single();

            Assert.That(one.Name, Is.EqualTo("one"));
            Assert.That(two.Name, Is.EqualTo("two"));
        }

        [Test]
        public void TransactionShouldCallActionAndCommitWhenSuccessful() {
            CreateThreeFoos();
            _fooRepos.Transaction(() => { _fooRepos.Get(f => f.Name == "one").Name = "uno"; });

            // need to evict all entities from session - otherwise modified class instances are selected
            _session.Clear();

            var foos = _fooRepos.Fetch(f => f.Name == "one" || f.Name == "uno");
            Assert.That(foos.Count(), Is.EqualTo(1));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("uno"));
            Assert.That(foos, Has.None.Property("Name").EqualTo("one"));
        }

        [Test]
        public void ExceptionsShouldRollbackTransactionAndRethrowOutOfMethod() {
            CreateThreeFoos();
            try {
                _fooRepos.Transaction(() => {
                    _fooRepos.Get(f => f.Name == "one").Name = "uno";
                    throw new ApplicationException("boom");
                });
            }
            catch (Exception ex) {
                Assert.That(ex.Message, Is.EqualTo("boom"));
            }

            // need to evict all entities from session - otherwise modified class instances are selected
            _session.Clear();

            var foos = _fooRepos.Fetch(f => f.Name == "one" || f.Name == "uno");
            Assert.That(foos.Count(), Is.EqualTo(1));
            Assert.That(foos, Has.None.Property("Name").EqualTo("uno"));
            Assert.That(foos, Has.Some.Property("Name").EqualTo("one"));
        }
    }
}