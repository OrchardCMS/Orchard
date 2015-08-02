﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Security;
using Orchard.Tests.ContentManagement;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;

namespace Orchard.Tests.Localization {
    [TestFixture]
    public class CultureManagerTests {
        private IContainer _container;
        private ICultureManager _cultureManager;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private string _databaseFileName;
        private StubWorkContext _stubWorkContext;

        [TestFixtureSetUp]
        public void InitFixture() {
            _databaseFileName = Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                _databaseFileName,
                typeof(CultureRecord));
        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _stubWorkContext = new StubWorkContext();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterInstance(_stubWorkContext);
            builder.RegisterInstance(new StubWorkContextAccessor(_stubWorkContext)).As<IWorkContextAccessor>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<DefaultCultureManager>().As<ICultureManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestTransactionManager(_session)).As<ITransactionManager>();
            _container = builder.Build();
            _cultureManager = _container.Resolve<ICultureManager>();
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
        public void CultureManagerCanAddAndListValidCultures() {
            _cultureManager.AddCulture("tr-TR");
            _cultureManager.AddCulture("fr-FR");
            _cultureManager.AddCulture("bs-Latn-BA");
            List<string> cultures = new List<string>(_cultureManager.ListCultures());
            Assert.That(cultures.Count, Is.Not.EqualTo(0));
        }

        [Test]
        public void CultureManagerRejectsInvalidCultureNames() {
            Assert.Throws<ArgumentException>(() => _cultureManager.AddCulture("a-b-c"));
        }

        [Test]
        public void CultureManagerAcceptsValidDotNetCultureNames() {
            foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures)) {
                if (!String.IsNullOrEmpty(cultureInfo.Name)) {
                    Assert.DoesNotThrow(() => _cultureManager.AddCulture(cultureInfo.Name));
                }
            }

            foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures)) {
                if (!String.IsNullOrEmpty(cultureInfo.Name)) {
                    Assert.DoesNotThrow(() => _cultureManager.AddCulture(cultureInfo.Name));
                }
            }
        }

        [Test]
        public void CultureManagerReturnsCultureFromWorkContext() {
            _stubWorkContext.CultureName = "nl-NL";
            Assert.That(_cultureManager.GetCurrentCulture(null), Is.EqualTo("nl-NL"));
        }
    }
}