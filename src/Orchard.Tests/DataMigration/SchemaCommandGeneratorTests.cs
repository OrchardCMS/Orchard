using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Features.Metadata;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Data.Conventions;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Records;
using Orchard.Data.Migration.Schema;
using Orchard.DevTools.Services;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Tests.ContentManagement;
using Orchard.Data.Providers;
using Orchard.Tests.FileSystems.AppData;

namespace Orchard.Tests.DataMigration {
    [TestFixture]
    public class SchemaCommandGeneratorTests  {
        private IContainer _container;
        private StubFolders _folders;
        private ISchemaCommandGenerator _generator;

        private ISessionFactory _sessionFactory;
        private ISession _session;

        [TestFixtureSetUp]
        public void CreateDb() {
            var types = new[] {
                typeof(BlogRecord),
                typeof(BodyRecord),
                typeof(BlogArchiveRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord)};

            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName, types );

            var builder = new ContainerBuilder();
            _folders = new StubFolders();

            var manager = (IDataServicesProviderFactory)new DataServicesProviderFactory(new[] {
                new Meta<CreateDataServicesProvider>(
                    (dataFolder, connectionString) => new SqlCeDataServicesProvider(dataFolder, connectionString),
                    new Dictionary<string, object> {{"ProviderName", "SqlCe"}})
            });

            builder.RegisterInstance(new ShellSettings { Name = "Default", DataTablePrefix = "TEST_", DataProvider = "SqlCe"});
            builder.RegisterInstance(AppDataFolderTests.CreateAppDataFolder(Path.GetDirectoryName(databaseFileName))).As<IAppDataFolder>();
            builder.RegisterType<SessionConfigurationCache>().As<ISessionConfigurationCache>();
            builder.RegisterType<SqlCeDataServicesProvider>().As<IDataServicesProvider>();
            builder.RegisterInstance(manager).As<IDataServicesProviderFactory>();
            builder.RegisterType<NullInterpreter>().As<IDataMigrationInterpreter>();
            builder.RegisterType<SessionFactoryHolder>().As<ISessionFactoryHolder>();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<SchemaCommandGenerator>().As<ISchemaCommandGenerator>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();

            builder.RegisterInstance(new ShellBlueprint() { Records = types.Select(t => new RecordBlueprint { Feature = new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature1" } }, TableName = "TEST_" + t.Name, Type = t })});

            _container = builder.Build();
            _generator = _container.Resolve<ISchemaCommandGenerator>();

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");
        }

        public class StubFolders : IExtensionFolders {
            public StubFolders() {
                Manifests = new Dictionary<string, string>();
            }

            public IDictionary<string, string> Manifests { get; set; }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                foreach (var e in Manifests) {
                    string name = e.Key;
                    var parseResult = ExtensionFolders.ParseManifest(Manifests[name]);
                    yield return ExtensionFolders.GetDescriptorForExtension("~/", name, "Module", parseResult);
                }
            }
        }


        [Test]
        public void ShouldCreateCreateTableCommands() {
            var commands = _generator.GetCreateFeatureCommands("Feature1", false).ToList();
            Assert.That(commands, Is.Not.Null);
            Assert.That(commands.Count(), Is.EqualTo(6));

            var blogRecord = commands.Where(c => c.Name == "TEST_BlogRecord").First();

            Assert.That(blogRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Id" && !c.IsIdentity && c.IsPrimaryKey && c.DbType == DbType.Int32));
            Assert.That(blogRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Description" && c.DbType == DbType.String));
            Assert.That(blogRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "PostCount" && c.DbType == DbType.Int32));

            var blogArchiveRecord = commands.Where(c => c.Name == "TEST_BlogArchiveRecord").First();
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Id" && c.IsPrimaryKey && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Year" && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Month" && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "PostCount" && c.DbType == DbType.Int32));
            Assert.That(blogArchiveRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Blog_id" && c.DbType == DbType.Int32));

            var bodyRecord = commands.Where(c => c.Name == "TEST_BodyRecord").First();
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Id" && c.IsPrimaryKey && c.DbType == DbType.Int32));
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Text" && c.DbType == DbType.String && c.Length == 10000));
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "Format" && c.DbType == DbType.String && c.Length == 42));
            Assert.That(bodyRecord.TableCommands.OfType<CreateColumnCommand>().Any(c => c.ColumnName == "ContentItemRecord_id" && c.DbType == DbType.Int32));
        }

        [Test]
        public void ScaffoldingCommandInterpreterShouldDetectContentParts() {

            var commands = _generator.GetCreateFeatureCommands("Feature1", false).ToList();
            Assert.That(commands, Is.Not.Null);
            Assert.That(commands.Count(), Is.EqualTo(6));

            var sw = new StringWriter();
            var interpreter = new ScaffoldingCommandInterpreter(sw);

            var blogRecord = commands.Where(c => c.Name == "TEST_BlogRecord").First();
            var blogArchiveRecord = commands.Where(c => c.Name == "TEST_BlogArchiveRecord").First();
            var bodyRecord = commands.Where(c => c.Name == "TEST_BodyRecord").First();

            sw.GetStringBuilder().Clear();
            interpreter.Visit(blogRecord);
            Assert.That(sw.ToString().Contains("SchemaBuilder.CreateTable(\"TEST_BlogRecord"));
            Assert.That(sw.ToString().Contains(".ContentPartRecord()"));
            Assert.That(sw.ToString().Contains(".Column(\"Description\", DbType.String)"));
            Assert.That(sw.ToString().Contains(".Column(\"PostCount\", DbType.Int32)"));

            sw.GetStringBuilder().Clear();
            interpreter.Visit(blogArchiveRecord);
            Assert.That(sw.ToString().Contains("SchemaBuilder.CreateTable(\"TEST_BlogArchiveRecord"));
            Assert.That(sw.ToString().Contains(".Column(\"Id\", DbType.Int32, column => column.PrimaryKey().Identity())"));
            Assert.That(sw.ToString().Contains(".Column(\"Year\", DbType.Int32)"));
            Assert.That(sw.ToString().Contains(".Column(\"Month\", DbType.Int32)"));
            Assert.That(sw.ToString().Contains(".Column(\"PostCount\", DbType.Int32)"));
            Assert.That(sw.ToString().Contains(".Column(\"Blog_id\", DbType.Int32)"));

            sw.GetStringBuilder().Clear();
            interpreter.Visit(bodyRecord);
            Assert.That(sw.ToString().Contains("SchemaBuilder.CreateTable(\"TEST_BodyRecord"));
            Assert.That(sw.ToString().Contains(".ContentPartVersionRecord()"));
            Assert.That(sw.ToString().Contains(".Column(\"Text\", DbType.String, column => column.WithLength(10000))"));
            Assert.That(sw.ToString().Contains(".Column(\"Format\", DbType.String, column => column.WithLength(42))"));
            Assert.That(!sw.ToString().Contains("ContentItemRecord_id"));
        }
    }


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