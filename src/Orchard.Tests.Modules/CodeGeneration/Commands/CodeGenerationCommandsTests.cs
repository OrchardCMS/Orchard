using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Features.Metadata;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.CodeGeneration.Commands;
using Orchard.Commands;
using Orchard.Data;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Providers;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Tests.Environment;
using Orchard.Tests.FileSystems.AppData;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.CodeGeneration.Commands {
    [TestFixture]
    public class CodeGenerationCommandsTests {

        private IContainer _container;
        private IExtensionManager _extensionManager;
        private ISchemaCommandGenerator _schemaCommandGenerator;

        [SetUp]
        public void Init() {
            string databaseFileName = Path.GetTempFileName();
            IDataServicesProviderFactory dataServicesProviderFactory = new DataServicesProviderFactory(new[] {
                new Meta<CreateDataServicesProvider>(
                    (dataFolder, connectionString) => new SqlCeDataServicesProvider(dataFolder, connectionString),
                    new Dictionary<string, object> {{"ProviderName", "SqlCe"}})
            });

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new ShellBlueprint());
            builder.RegisterInstance(new ShellSettings { Name = ShellSettings.DefaultName, DataTablePrefix = "Test", DataProvider = "SqlCe" });
            builder.RegisterInstance(dataServicesProviderFactory).As<IDataServicesProviderFactory>();
            builder.RegisterInstance(AppDataFolderTests.CreateAppDataFolder(Path.GetDirectoryName(databaseFileName))).As<IAppDataFolder>();

            builder.RegisterType<SqlCeDataServicesProvider>().As<IDataServicesProvider>();
            builder.RegisterType<SessionConfigurationCache>().As<ISessionConfigurationCache>();
            builder.RegisterType<SessionFactoryHolder>().As<ISessionFactoryHolder>();
            builder.RegisterType<DefaultDatabaseCacheConfiguration>().As<IDatabaseCacheConfiguration>();
            builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<SchemaCommandGenerator>().As<ISchemaCommandGenerator>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();

            _container = builder.Build();
            _extensionManager = _container.Resolve<IExtensionManager>();
            _schemaCommandGenerator = _container.Resolve<ISchemaCommandGenerator>();
        }

        [Test]
        public void CreateDataMigrationTestNonExistentFeature() {
            CodeGenerationCommands codeGenerationCommands = new CodeGenerationCommands(_extensionManager,
                _schemaCommandGenerator);

            TextWriter textWriterOutput = new StringWriter();
            codeGenerationCommands.Context = new CommandContext { Output = textWriterOutput };
            codeGenerationCommands.CreateDataMigration("feature");

            Assert.That(textWriterOutput.ToString(), Is.StringContaining("Creating data migration failed"));
        }
    }
}