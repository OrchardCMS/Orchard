using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.WebSite;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Events;
using Orchard.Recipes.Services;
using Orchard.Services;
using Orchard.Tests.ContentManagement;
using Orchard.Tests.Environment.Extensions;
using Orchard.Tests.Modules.Recipes.Services;
using Orchard.Tests.Stubs;
using Orchard.Tests.UI.Navigation;

namespace Orchard.Tests.Modules.ImportExport.Services {
    [TestFixture]
    public class ImportExportManagerTests {
        private IContainer _container;
        private IImportExportService _importExportService;
        private ISessionFactory _sessionFactory;
        private ISession _session;

        [TestFixtureSetUp]
        public void InitFixture() {
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(ContentTypeRecord),
                typeof(ContentItemRecord),
                typeof(ContentItemVersionRecord));
        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImportExportService>().As<IImportExportService>();
            builder.RegisterType<StubShellDescriptorManager>().As<IShellDescriptorManager>();
            builder.RegisterType<RecipeManager>().As<IRecipeManager>();
            builder.RegisterType<RecipeHarvester>().As<IRecipeHarvester>();
            builder.RegisterType<RecipeStepExecutor>().As<IRecipeStepExecutor>();
            builder.RegisterType<StubStepQueue>().As<IRecipeStepQueue>().InstancePerLifetimeScope();
            builder.RegisterType<StubRecipeJournal>().As<IRecipeJournal>();
            builder.RegisterType<StubRecipeScheduler>().As<IRecipeScheduler>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<ExtensionManagerTests.StubLoaders>().As<IExtensionLoader>();
            builder.RegisterType<RecipeParser>().As<IRecipeParser>();
            builder.RegisterType<StubWebSiteFolder>().As<IWebSiteFolder>();
            builder.RegisterType<CustomRecipeHandler>().As<IRecipeHandler>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<ContentDefinitionWriter>().As<IContentDefinitionWriter>();
            builder.RegisterType<StubOrchardServices>().As<IOrchardServices>();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterInstance(new Mock<ISettingsFormatter>().Object);
            builder.RegisterInstance(new Mock<IRecipeExecuteEventHandler>().Object);
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestTransactionManager(_session)).As<ITransactionManager>();

            _container = builder.Build();
            _importExportService = _container.Resolve<IImportExportService>();
        }

        [Test]
        public void ImportSucceedsWhenRecipeContainsImportSteps() {
            Assert.DoesNotThrow(() => _importExportService.Import(
                                                                    @"<Orchard>
                                                                        <Recipe>
                                                                        <Name>MyModuleInstaller</Name>
                                                                        </Recipe>
                                                                        <Settings />
                                                                    </Orchard>"));
        }

        [Test]
        public void ImportDoesntFailsWhenRecipeContainsNonImportSteps() {
            Assert.DoesNotThrow(() => _importExportService.Import(
                                                                    @"<Orchard>
                                                                        <Recipe>
                                                                        <Name>MyModuleInstaller</Name>
                                                                        </Recipe>
                                                                        <Module name=""MyModule"" />
                                                                    </Orchard>"));
        }
    }

    public class StubShellDescriptorManager : IShellDescriptorManager {
        public ShellDescriptor GetShellDescriptor() {
            return new ShellDescriptor();
        }

        public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
        }
    }
}