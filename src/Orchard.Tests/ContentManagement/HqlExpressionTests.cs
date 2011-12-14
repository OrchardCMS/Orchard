using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using Orchard.Tests.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Records;
using Orchard.Tests.ContentManagement.Models;
using Orchard.DisplayManagement.Implementation;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.ContentManagement {
    [TestFixture]
    public class HqlExpressionTests {
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
                typeof(EpsilonRecord),
                typeof(LambdaRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord));
        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new ContentModule());
            builder.RegisterType<DefaultContentManager>().As<IContentManager>().SingleInstance();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);

            builder.RegisterType<AlphaPartHandler>().As<IContentHandler>();
            builder.RegisterType<BetaPartHandler>().As<IContentHandler>();
            builder.RegisterType<GammaPartHandler>().As<IContentHandler>();
            builder.RegisterType<DeltaPartHandler>().As<IContentHandler>();
            builder.RegisterType<EpsilonPartHandler>().As<IContentHandler>();
            builder.RegisterType<LambdaPartHandler>().As<IContentHandler>();
            builder.RegisterType<FlavoredPartHandler>().As<IContentHandler>();
            builder.RegisterType<StyledHandler>().As<IContentHandler>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));

            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();

            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();

            _session.Delete(string.Format("from {0}", typeof(GammaRecord).FullName));
            _session.Delete(string.Format("from {0}", typeof(DeltaRecord).FullName));
            _session.Delete(string.Format("from {0}", typeof(EpsilonRecord).FullName));
            _session.Delete(string.Format("from {0}", typeof(LambdaRecord).FullName));
            
            _session.Delete(string.Format("from {0}", typeof(ContentItemVersionRecord).FullName));
            _session.Delete(string.Format("from {0}", typeof(ContentItemRecord).FullName));
            _session.Delete(string.Format("from {0}", typeof(ContentTypeRecord).FullName));
            _session.Flush();
            _session.Clear();

            _container = builder.Build();
            _manager = _container.Resolve<IContentManager>();

        }

        [Test]
        public void AllDataTypesCanBeQueried() {
            var dt = DateTime.Now;

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = true;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var lambda = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("BooleanStuff", true)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("DecimalStuff", (decimal)0.0)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("DoubleStuff", 0.0)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("FloatStuff", (float)0.0)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("IntegerStuff", 0)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("LongStuff", (long)0)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("StringStuff", "0")).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));

            lambda = _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), x => x.Eq("DateTimeStuff", dt)).List();
            Assert.That(lambda.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorLike() {
            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.StringStuff = "abcdef";
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.Like("StringStuff", "bc", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Like("StringStuff", "ab", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Like("StringStuff", "ef", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Like("StringStuff", "gh", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Like("StringStuff", "ab", HqlMatchMode.Start));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Like("StringStuff", "ef", HqlMatchMode.End));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Like("StringStuff", "abcdef", HqlMatchMode.Exact));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Like("StringStuff", "abcde", HqlMatchMode.Exact));
            Assert.That(result.Count(), Is.EqualTo(0));

            // default collation in SQL Ce/Server but can be changed during db creation
            result = queryWhere(x => x.Like("StringStuff", "EF", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorInsensitiveLike() {
            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.StringStuff = "abcdef";
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "bc", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "ab", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "ef", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "gh", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "ab", HqlMatchMode.Start));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "ef", HqlMatchMode.End));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "abcdef", HqlMatchMode.Exact));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "abcde", HqlMatchMode.Exact));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.InsensitiveLike("StringStuff", "EF", HqlMatchMode.Anywhere));
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorGt() {
            var dt = new DateTime(1980,1,1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = true;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.Gt("BooleanStuff", true));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("DecimalStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("DoubleStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("FloatStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("IntegerStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("LongStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("StringStuff", "0"));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("DateTimeStuff", dt));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Gt("BooleanStuff", false));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("DecimalStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("DoubleStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("FloatStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("IntegerStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("LongStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("StringStuff", ""));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Gt("DateTimeStuff", dt.AddDays(-1)));
            Assert.That(result.Count(), Is.EqualTo(1));

        }

        [Test]
        public void ShouldQueryUsingOperatorLt() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.Lt("BooleanStuff", false));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("DecimalStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("DoubleStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("FloatStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("IntegerStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("LongStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("StringStuff", "0"));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("DateTimeStuff", dt));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Lt("BooleanStuff", true));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("DecimalStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("DoubleStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("FloatStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("IntegerStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("LongStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("StringStuff", "00"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Lt("DateTimeStuff", dt.AddDays(1)));
            Assert.That(result.Count(), Is.EqualTo(1));

        }


        [Test]
        public void ShouldQueryUsingOperatorLe() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal
            result = queryWhere(x => x.Le("BooleanStuff", false));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("DecimalStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("DoubleStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("FloatStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("IntegerStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("LongStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("StringStuff", "0"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("DateTimeStuff", dt));
            Assert.That(result.Count(), Is.EqualTo(1));

            // greater values
            result = queryWhere(x => x.Le("BooleanStuff", true));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("DecimalStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("DoubleStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("FloatStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("IntegerStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("LongStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("StringStuff", "00"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Le("DateTimeStuff", dt.AddDays(1)));
            Assert.That(result.Count(), Is.EqualTo(1));

            // lower values
            result = queryWhere(x => x.Le("DecimalStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Le("DoubleStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Le("FloatStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Le("IntegerStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Le("LongStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Le("StringStuff", ""));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Le("DateTimeStuff", dt.AddDays(-1)));
            Assert.That(result.Count(), Is.EqualTo(0));

        }

        [Test]
        public void ShouldQueryUsingOperatorGe() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal
            result = queryWhere(x => x.Ge("BooleanStuff", false));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("DecimalStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("DoubleStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("FloatStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("IntegerStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("LongStuff", 0));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("StringStuff", "0"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("DateTimeStuff", dt));
            Assert.That(result.Count(), Is.EqualTo(1));

            // greater values
            result = queryWhere(x => x.Ge("BooleanStuff", true));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("DecimalStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("DoubleStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("FloatStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("IntegerStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("LongStuff", 1));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("StringStuff", "00"));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Ge("DateTimeStuff", dt.AddDays(1)));
            Assert.That(result.Count(), Is.EqualTo(0));

            // lower values
            result = queryWhere(x => x.Ge("DecimalStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("DoubleStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("FloatStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("IntegerStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("LongStuff", -1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("StringStuff", ""));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Ge("DateTimeStuff", dt.AddDays(-1)));
            Assert.That(result.Count(), Is.EqualTo(1));

        }

        [Test]
        public void ShouldQueryUsingOperatorBetween() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // include
            result = queryWhere(x => x.Between("DecimalStuff", 0, 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Between("DoubleStuff", 0, 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Between("FloatStuff", 0, 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Between("IntegerStuff", 0, 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Between("LongStuff", 0, 1));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Between("StringStuff", "0", "1"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.Between("DateTimeStuff", dt, dt.AddDays(1)));
            Assert.That(result.Count(), Is.EqualTo(1));

            // exclude
            result = queryWhere(x => x.Between("DecimalStuff", 1, 2));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Between("DoubleStuff", 1, 2));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Between("FloatStuff", 1, 2));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Between("IntegerStuff", 1, 2));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Between("LongStuff", 1, 2));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Between("StringStuff", "1", "2"));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.Between("DateTimeStuff", dt.AddDays(1), dt.AddDays(2)));
            Assert.That(result.Count(), Is.EqualTo(0));

        }

        [Test]
        public void ShouldQueryUsingOperatorIn() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // include
            result = queryWhere(x => x.In("BooleanStuff", new[] { false }));
            Assert.That(result.Count(), Is.EqualTo(1));
            
            result = queryWhere(x => x.In("DecimalStuff", new[] { 0, 1 }));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.In("DoubleStuff", new[] { 0, 1 }));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.In("FloatStuff", new[] { 0, 1 }));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.In("IntegerStuff", new[] { 0, 1 }));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.In("LongStuff", new[] { 0, 1 }));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.In("StringStuff", new[] { "0", "1" }));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.In("DateTimeStuff", new [] {dt, dt.AddDays(1)}));
            Assert.That(result.Count(), Is.EqualTo(1));

            // exclude
            result = queryWhere(x => x.In("BooleanStuff", new[] { true }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("DecimalStuff", new[] { 1, 2 }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("DoubleStuff", new[] { 1, 2 }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("FloatStuff", new[] { 1, 2 }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("IntegerStuff", new[] { 1, 2 }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("LongStuff", new[] { 1, 2 }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("StringStuff", new[] { "1", "2" }));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.In("DateTimeStuff", new [] {dt.AddDays(1), dt.AddDays(2)}));
            Assert.That(result.Count(), Is.EqualTo(0));

        }

        [Test]
        public void ShouldQueryUsingOperatorIsNull() {
            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.StringStuff = null;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.IsNull("BooleanStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.IsNull("StringStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorIsNotNull() {
            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.StringStuff = null;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.IsNotNull("BooleanStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.IsNotNull("StringStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldQueryUsingOperatorEqProperty() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.EqProperty("BooleanStuff", "LongStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));

            result = queryWhere(x => x.EqProperty("DateTimeStuff", "LongStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldQueryUsingOperatorNotEqProperty() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 0;
                init.Record.LongStuff = 0;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            result = queryWhere(x => x.NotEqProperty("BooleanStuff", "LongStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));

            result = queryWhere(x => x.NotEqProperty("DateTimeStuff", "LongStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorGtProperty() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 1;
                init.Record.LongStuff = 2;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal 
            result = queryWhere(x => x.GtProperty("DoubleStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));

            // lesser
            result = queryWhere(x => x.GtProperty("FloatStuff", "IntegerStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));

            // greater
            result = queryWhere(x => x.GtProperty("IntegerStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));
        }


        [Test]
        public void ShouldQueryUsingOperatorGeProperty() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 1;
                init.Record.LongStuff = 2;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal 
            result = queryWhere(x => x.GeProperty("DoubleStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));

            // lesser
            result = queryWhere(x => x.GeProperty("FloatStuff", "IntegerStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));

            // greater
            result = queryWhere(x => x.GeProperty("IntegerStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorLeProperty() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 1;
                init.Record.LongStuff = 2;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal 
            result = queryWhere(x => x.LeProperty("DoubleStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));

            // lesser
            result = queryWhere(x => x.LeProperty("FloatStuff", "IntegerStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));

            // greater
            result = queryWhere(x => x.LeProperty("IntegerStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));
        }


        [Test]
        public void ShouldQueryUsingOperatorLtProperty() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 1;
                init.Record.LongStuff = 2;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal 
            result = queryWhere(x => x.LtProperty("DoubleStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));

            // lesser
            result = queryWhere(x => x.LtProperty("FloatStuff", "IntegerStuff"));
            Assert.That(result.Count(), Is.EqualTo(1));

            // greater
            result = queryWhere(x => x.LtProperty("IntegerStuff", "FloatStuff"));
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldSortRandomly() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.IntegerStuff = 1;
            });

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.IntegerStuff = 2;
            });

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.IntegerStuff = 3;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(3));

            var firstResults = new List<int>();

            for (int i = 0; i < 10; i++) {
                result = _manager.HqlQuery().Join(alias => alias.ContentPartRecord<LambdaRecord>()).OrderBy(x => x.Named("civ"), order => order.Random()).List();
                firstResults.Add(result.First().As<LambdaPart>().Record.IntegerStuff);
            }

            Assert.That(firstResults.Distinct().Count(), Is.GreaterThan(1));
        }

        [Test]
        public void ShouldQueryUsingOperatorNot() {
            var dt = new DateTime(1980, 1, 1);

            _manager.Create<LambdaPart>("lambda", init => {
                init.Record.BooleanStuff = false;
                init.Record.DecimalStuff = 0;
                init.Record.DoubleStuff = 0;
                init.Record.FloatStuff = 0;
                init.Record.IntegerStuff = 1;
                init.Record.LongStuff = 2;
                init.Record.StringStuff = "0";
                init.Record.DateTimeStuff = dt;
            });
            _session.Flush();

            var result = _manager.HqlQuery().ForType("lambda").List();
            Assert.That(result.Count(), Is.EqualTo(1));

            Func<Action<IHqlExpressionFactory>, IEnumerable<ContentItem>> queryWhere = predicate => _manager.HqlQuery().Where(alias => alias.ContentPartRecord<LambdaRecord>(), predicate).List();

            // equal 
            result = queryWhere(x => x.Not(y => y.LtProperty("DoubleStuff", "FloatStuff")));
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}



