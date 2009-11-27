using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Builder;
using Autofac.Modules;
using NHibernate;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Tests.Models.Records;
using Orchard.Tests.Models.Stubs;

namespace Orchard.Tests.Models {
    [TestFixture]
    public class ContentQueryTests {
        private IContainer _container;
        private IContentManager _manager;
        private ISessionFactory _sessionFactory;
        private ISession _session;

        [TestFixtureSetUp]
        public void InitFixture() {
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(GammaRecord),
                typeof(DeltaRecord),
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
            builder.RegisterModule(new ContentModule());
            builder.Register<DefaultContentManager>().As<IContentManager>();
            builder.Register<AlphaProvider>().As<IContentProvider>();
            builder.Register<BetaProvider>().As<IContentProvider>();
            builder.Register<GammaProvider>().As<IContentProvider>();
            builder.Register<DeltaProvider>().As<IContentProvider>();
            builder.Register<FlavoredProvider>().As<IContentProvider>();
            builder.Register<StyledProvider>().As<IContentProvider>();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));

            _session = _sessionFactory.OpenSession();
            builder.Register(new DefaultModelManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();

            _session.Delete("from GammaRecord");
            _session.Delete("from DeltaRecord");
            _session.Delete("from ContentItemRecord");
            _session.Delete("from ContentTypeRecord");
            _session.Flush();
            _session.Clear();

            _container = builder.Build();
            _manager = _container.Resolve<IContentManager>();

        }

        private void AddSampleData() {
            _manager.Create<Alpha>("alpha", init => { });
            _manager.Create<Beta>("beta", init => { });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "the frap value"; });
            _manager.Create<Delta>("delta", init => { init.Record.Quux = "the quux value"; });
            _session.Flush();
        }

        [Test]
        public void QueryInstanceIsDifferentEachTimeYouCreateOne() {
            var contentManager1 = _container.Resolve<IContentManager>();
            var query1a = contentManager1.Query();
            var query1b = contentManager1.Query();

            var contentManager2 = _container.Resolve<IContentManager>();
            var query2a = contentManager2.Query();
            var query2b = contentManager2.Query();

            Assert.That(contentManager1, Is.SameAs(contentManager2));
            Assert.That(query1a, Is.SameAs(query1a));

            Assert.That(query1a, Is.Not.SameAs(query1b));
            Assert.That(query1a, Is.Not.SameAs(query2a));
            Assert.That(query1a, Is.Not.SameAs(query2b));

            Assert.That(query1b, Is.Not.SameAs(query2a));
            Assert.That(query1b, Is.Not.SameAs(query2b));

            Assert.That(query2a, Is.Not.SameAs(query2b));
        }

        [Test]
        public void ContentManagerPropertyIsSet() {
            var contentManager = _container.Resolve<IContentManager>();
            var query = contentManager.Query();
            Assert.That(query.ContentManager, Is.SameAs(contentManager));

            var mockManager = new Moq.Mock<IContentManager>().Object;
            var anotherQuery = _container.Resolve<IContentQuery>(TypedParameter.From(mockManager));
            Assert.That(anotherQuery, Is.Not.SameAs(query));
            Assert.That(anotherQuery.ContentManager, Is.SameAs(mockManager));
        }

        [Test]
        public void AllItemsAreReturnedByDefault() {
            AddSampleData();

            var allItems = _manager.Query().List();

            Assert.That(allItems.Count(), Is.EqualTo(4));
            Assert.That(allItems.Count(x => x.Has<Alpha>()), Is.EqualTo(1));
            Assert.That(allItems.Count(x => x.Has<Beta>()), Is.EqualTo(1));
            Assert.That(allItems.Count(x => x.Has<Gamma>()), Is.EqualTo(1));
            Assert.That(allItems.Count(x => x.Has<Delta>()), Is.EqualTo(1));
        }

        [Test]
        public void SpecificTypeIsReturnedWhenSpecified() {
            AddSampleData();

            var alphaBeta = _manager.Query().ForType("alpha", "beta").List();

            Assert.That(alphaBeta.Count(), Is.EqualTo(2));
            Assert.That(alphaBeta.Count(x => x.Has<Alpha>()), Is.EqualTo(1));
            Assert.That(alphaBeta.Count(x => x.Has<Beta>()), Is.EqualTo(1));
            Assert.That(alphaBeta.Count(x => x.Has<Gamma>()), Is.EqualTo(0));
            Assert.That(alphaBeta.Count(x => x.Has<Delta>()), Is.EqualTo(0));

            var gammaDelta = _manager.Query().ForType("gamma", "delta").List();

            Assert.That(gammaDelta.Count(), Is.EqualTo(2));
            Assert.That(gammaDelta.Count(x => x.Has<Alpha>()), Is.EqualTo(0));
            Assert.That(gammaDelta.Count(x => x.Has<Beta>()), Is.EqualTo(0));
            Assert.That(gammaDelta.Count(x => x.Has<Gamma>()), Is.EqualTo(1));
            Assert.That(gammaDelta.Count(x => x.Has<Delta>()), Is.EqualTo(1));
        }

        [Test]
        public void WherePredicateRestrictsResults() {
            AddSampleData();
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "one"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "two"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "three"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "four"; });
            _session.Flush();

            var twoOrFour = _manager.Query<Gamma, GammaRecord>()
                .Where(x => x.Frap == "one" || x.Frap == "four")
                .List();

            Assert.That(twoOrFour.Count(), Is.EqualTo(2));
            Assert.That(twoOrFour.Count(x => x.Has<Gamma>()), Is.EqualTo(2));
            Assert.That(twoOrFour.Count(x => x.Get<Gamma>().Record.Frap == "one"), Is.EqualTo(1));
            Assert.That(twoOrFour.Count(x => x.Get<Gamma>().Record.Frap == "four"), Is.EqualTo(1));
        }


        [Test]
        public void EmptyWherePredicateRequiresRecord() {
            AddSampleData();
            var gammas = _manager.Query().Join<GammaRecord>().List();
            var deltas = _manager.Query().Join<DeltaRecord>().List();

            Assert.That(gammas.Count(), Is.EqualTo(1));
            Assert.That(deltas.Count(), Is.EqualTo(1));
            Assert.That(gammas.AsPart<Gamma>().Single().Record.Frap, Is.EqualTo("the frap value"));
            Assert.That(deltas.AsPart<Delta>().Single().Record.Quux, Is.EqualTo("the quux value"));
        }

        [Test]
        public void OrderMaySortOnJoinedRecord() {
            AddSampleData();
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "one"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "two"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "three"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "four"; });
            _session.Flush();

            var ascending = _manager.Query("gamma")
                .OrderBy<GammaRecord, string>(x => x.Frap)
                .List<Gamma>();

            Assert.That(ascending.Count(), Is.EqualTo(5));
            Assert.That(ascending.First().Record.Frap, Is.EqualTo("four"));
            Assert.That(ascending.Last().Record.Frap, Is.EqualTo("two"));


            var descending = _manager.Query<Gamma, GammaRecord>()
                .OrderByDescending(x => x.Frap)
                .List();

            Assert.That(descending.Count(), Is.EqualTo(5));
            Assert.That(descending.First().Record.Frap, Is.EqualTo("two"));
            Assert.That(descending.Last().Record.Frap, Is.EqualTo("four"));
        }

        [Test]
        public void SkipAndTakeProvidePagination() {
            AddSampleData();
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "one"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "two"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "three"; });
            _manager.Create<Gamma>("gamma", init => { init.Record.Frap = "four"; });
            _session.Flush();

            var reverseById = _manager.Query()
                .OrderByDescending<GammaRecord, int>(x => x.Id)
                .List();

            var subset = _manager.Query()
                .OrderByDescending<GammaRecord, int>(x => x.Id)
                .Slice(2, 3);

            Assert.That(subset.Count(), Is.EqualTo(3));
            Assert.That(subset.First().Id, Is.EqualTo(reverseById.Skip(2).First().Id));
            Assert.That(subset.Skip(1).First().Id, Is.EqualTo(reverseById.Skip(3).First().Id));
            Assert.That(subset.Skip(2).First().Id, Is.EqualTo(reverseById.Skip(4).First().Id));

        }
    }
}



