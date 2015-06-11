using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.CodeGeneration.Services;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Data.Conventions;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.Dependencies;
using Orchard.Tests.ContentManagement;
using Orchard.Data.Providers;
using Orchard.Tests.DataMigration.Utilities;
using Orchard.Tests.FileSystems.AppData;
using Orchard.Tests.Modules.Migrations.Orchard.Tests.DataMigration.Records;
using Path = Bleroy.FluentPath.Path;
using Orchard.Tests.Stubs;
using Orchard.Tests.Environment;
using Orchard.Environment;

namespace Orchard.Tests.Modules.Migrations {
    [TestFixture]
    public class SchemaCommandGeneratorTests {
        private IContainer _container;
        private StubFolders _folders;
        private ISchemaCommandGenerator _generator;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private readonly Path _tempFixtureFolderName = Path.Get(System.IO.Path.GetTempPath()).Combine("Orchard.Tests.Modules.Migrations");
        private Path _tempFolderName;

        [TestFixtureSetUp]
        public void CreateDb() {
            var types = new[] {
                typeof(BlogRecord),
                typeof(BodyRecord),
                typeof(BlogArchiveRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord)};

            _tempFolderName = _tempFixtureFolderName.Combine(System.IO.Path.GetRandomFileName());
            try {
                _tempFixtureFolderName.Delete(true);
            } catch {}
            _tempFixtureFolderName.CreateDirectory();
            _sessionFactory = DataUtility.CreateSessionFactory(
                _tempFolderName, types);

            var builder = new ContainerBuilder();
            _folders = new StubFolders();

            var manager = (IDataServicesProviderFactory)new DataServicesProviderFactory(new[] {
                new Meta<CreateDataServicesProvider>(
                    (dataFolder, connectionString) => new SqlCeDataServicesProvider(dataFolder, connectionString),
                    new Dictionary<string, object> {{"ProviderName", "SqlCe"}})
            });

            builder.RegisterInstance(new ShellSettings { Name = ShellSettings.DefaultName, DataTablePrefix = "TEST", DataProvider = "SqlCe" });
            builder.RegisterInstance(AppDataFolderTests.CreateAppDataFolder(_tempFixtureFolderName)).As<IAppDataFolder>();
            builder.RegisterType<SessionConfigurationCache>().As<ISessionConfigurationCache>();
            builder.RegisterType<SqlCeDataServicesProvider>().As<IDataServicesProvider>();
            builder.RegisterInstance(manager).As<IDataServicesProviderFactory>();
            builder.RegisterType<NullInterpreter>().As<IDataMigrationInterpreter>();
            builder.RegisterType<SessionFactoryHolder>().As<ISessionFactoryHolder>();
            builder.RegisterType<DefaultDatabaseCacheConfiguration>().As<IDatabaseCacheConfiguration>();
            builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<StubLoaders>().As<IExtensionLoader>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<SchemaCommandGenerator>().As<ISchemaCommandGenerator>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();

            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();

            builder.RegisterInstance(new ShellBlueprint());

            _container = builder.Build();
            _generator = _container.Resolve<ISchemaCommandGenerator>();

            _folders.Manifests.Add("Feature1", @"
Name: Module1
Version: 0.1
OrchardVersion: 1
Features:
    Feature1: 
        Description: Feature
");
        }

        [TestFixtureTearDown]
        public void Term() {
            try { _tempFixtureFolderName.Delete(true); }
            catch { }
        }

        public class StubFolders : IExtensionFolders {
            public StubFolders() {
                Manifests = new Dictionary<string, string>();
            }

            public IDictionary<string, string> Manifests { get; set; }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                foreach (var e in Manifests) {
                    string name = e.Key;
                    yield return ExtensionHarvester.GetDescriptorForExtension("~/", name, DefaultExtensionTypes.Module, Manifests[name]);
                }
            }
        }

        public class StubLoaders : IExtensionLoader {
            #region Implementation of IExtensionLoader

            public int Order {
                get { return 1; }
            }

            public string Name {
                get { return this.GetType().Name; }
            }

            public Assembly LoadReference(DependencyReferenceDescriptor reference) {
                throw new NotImplementedException();
            }

            public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
                throw new NotImplementedException();
            }

            public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
                throw new NotImplementedException();
            }

            public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
                throw new NotImplementedException();
            }

            public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
                return new ExtensionProbeEntry { Descriptor = descriptor, Loader = this };
            }

            public IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor extensionDescriptor) {
                throw new NotImplementedException();
            }

            public ExtensionEntry Load(ExtensionDescriptor descriptor) {
                return new ExtensionEntry { Descriptor = descriptor, ExportedTypes = new[] { typeof(BlogRecord), typeof(BodyRecord), typeof(BlogArchiveRecord) } };
            }

            public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
                throw new NotImplementedException();
            }

            public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
                throw new NotImplementedException();
            }

            public void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
                throw new NotImplementedException();
            }

            public void Monitor(ExtensionDescriptor extension, Action<IVolatileToken> monitor) {
            }

            public IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency) {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor dependency) {
                throw new NotImplementedException();
            }

            #endregion
        }

        [Test]
        public void ShouldCreateCreateTableCommands() {
            var commands = _generator.GetCreateFeatureCommands("Feature1", false).ToList();
            Assert.That(commands, Is.Not.Null);
            Assert.That(commands.Count(), Is.EqualTo(3));

            var blogRecord = commands.Where(c => c.Name == "TEST_Feature1_BlogRecord").First();

            Assert.That(blogRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Id" && !c.IsIdentity && c.IsPrimaryKey && c.DbType == DbType.Int32));
            Assert.That(blogRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Description" && c.DbType == DbType.String));
            Assert.That(blogRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "PostCount" && c.DbType == DbType.Int32));

            var blogArchiveRecord = commands.Where(c => c.Name == "TEST_Feature1_BlogArchiveRecord").First();
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Id" && c.IsPrimaryKey && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Year" && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Month" && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "PostCount" && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Blog_id" && c.DbType == DbType.Int32));

            var bodyRecord = commands.Where(c => c.Name == "TEST_Feature1_BodyRecord").First();
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Id" && c.IsPrimaryKey && c.DbType == DbType.Int32));
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Text" && c.DbType == DbType.String && c.Length == 10000));
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Format" && c.DbType == DbType.String && c.Length == 42));
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "ContentItemRecord_id" && c.DbType == DbType.Int32));
        }

        [Test]
        public void ScaffoldingCommandInterpreterShouldDetectContentParts() {

            var commands = _generator.GetCreateFeatureCommands("Feature1", false).ToList();
            Assert.That(commands, Is.Not.Null);
            Assert.That(commands.Count(), Is.EqualTo(3));

            var sw = new StringWriter();
            var interpreter = new CodeGenerationCommandInterpreter(sw);

            var blogRecord = commands.Where(c => c.Name == "TEST_Feature1_BlogRecord").First();
            var blogArchiveRecord = commands.Where(c => c.Name == "TEST_Feature1_BlogArchiveRecord").First();
            var bodyRecord = commands.Where(c => c.Name == "TEST_Feature1_BodyRecord").First();

            sw.GetStringBuilder().Clear();
            interpreter.Visit(blogRecord);
            Assert.That(sw.ToString().Contains("SchemaBuilder.CreateTable(\"TEST_Feature1_BlogRecord"));
            Assert.That(sw.ToString().Contains(".ContentPartRecord()"));
            Assert.That(sw.ToString().Contains(".Column(\"Description\", DbType.String)"));
            Assert.That(sw.ToString().Contains(".Column(\"PostCount\", DbType.Int32)"));

            sw.GetStringBuilder().Clear();
            interpreter.Visit(blogArchiveRecord);
            Assert.That(sw.ToString().Contains("SchemaBuilder.CreateTable(\"TEST_Feature1_BlogArchiveRecord"));
            Assert.That(sw.ToString().Contains(".Column(\"Id\", DbType.Int32, column => column.PrimaryKey().Identity())"));
            Assert.That(sw.ToString().Contains(".Column(\"Year\", DbType.Int32)"));
            Assert.That(sw.ToString().Contains(".Column(\"Month\", DbType.Int32)"));
            Assert.That(sw.ToString().Contains(".Column(\"PostCount\", DbType.Int32)"));
            Assert.That(sw.ToString().Contains(".Column(\"Blog_id\", DbType.Int32)"));

            sw.GetStringBuilder().Clear();
            interpreter.Visit(bodyRecord);
            Assert.That(sw.ToString().Contains("SchemaBuilder.CreateTable(\"TEST_Feature1_BodyRecord"));
            Assert.That(sw.ToString().Contains(".ContentPartVersionRecord()"));
            Assert.That(sw.ToString().Contains(".Column(\"Text\", DbType.String, column => column.Unlimited())"));
            Assert.That(sw.ToString().Contains(".Column(\"Format\", DbType.String, column => column.WithLength(42))"));
            Assert.That(!sw.ToString().Contains("ContentItemRecord_id"));
        }
    }


    // namespace is needed as the shell composition strategy will filter records using it also
    namespace Orchard.Tests.DataMigration.Records {
        public class BlogRecord : ContentPartRecord {
            public virtual string Description { get; set; }
            public virtual int PostCount { get; set; }
        }

        public class BodyRecord : ContentPartVersionRecord {
            [StringLengthMax]
            public virtual string Text { get; set; }
            [StringLength(42)]
            public virtual string Format { get; set; }
        }

        public class BlogArchiveRecord {
            public virtual int Id { get; set; }
            public virtual BlogRecord Blog { get; set; }
            public virtual int Year { get; set; }
            public virtual int Month { get; set; }
            public virtual int PostCount { get; set; }
        }
    }
}
