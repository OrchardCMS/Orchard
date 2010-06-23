using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.ContentManagement;
using Orchard.DataMigration;

namespace Orchard.Tests.DataMigration {
    [TestFixture]
    public class DataMigrationTests {
        private IContainer _container;
        private IExtensionManager _manager;
        private StubFolders _folders;
        private IDataMigrationManager _dataMigrationManager;
        private IRepository<DataMigrationRecord> _repository;

        private ISessionFactory _sessionFactory;
        private ISession _session;

        [SetUp]
        public void Init() {
            Init(Enumerable.Empty<Type>());
        }

        public void Init(IEnumerable<Type> dataMigrations) {

            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(DataMigrationRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord));

            var builder = new ContainerBuilder();
            _folders = new StubFolders();
            
            builder.RegisterInstance(new ShellSettings { DataTablePrefix = "TEST_"});

            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<DataMigrationManager>().As<IDataMigrationManager>();
            builder.RegisterType<DefaultDataMigrationGenerator>().As<IDataMigrationGenerator>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();
            foreach(var type in dataMigrations) {
                builder.RegisterType(type).As<IDataMigration>();
            }
            _container = builder.Build();
            _manager = _container.Resolve<IExtensionManager>();
            _dataMigrationManager = _container.Resolve<IDataMigrationManager>();
            _repository = _container.Resolve<IRepository<DataMigrationRecord>>();

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

        public class DataMigrationEmpty : IDataMigration {
            public string Feature {
                get { return "Feature1"; }
            }
        }

        public class DataMigration11 : IDataMigration {
            public string Feature {
                get { return "Feature1"; }
            }
        }

        public class DataMigration11Create : IDataMigration {
            public string Feature {
                get { return "Feature1"; }
            }

            public int Create() {
                return 999;
            }
        }

        public class DataMigrationCreateCanBeFollowedByUpdates : IDataMigration {
            public string Feature {
                get { return "Feature1"; }
            }

            public int Create() {
                return 42;
            }

            public int UpdateFrom42() {
                return 666;
            }
        }

        public class DataMigrationSameMigrationClassCanEvolve : IDataMigration {
            public string Feature {
                get { return "Feature1"; }
            }

            public int Create() {
                return 999;
            }

            public int UpdateFrom42() {
                return 666;
            }

            public int UpdateFrom666() {
                return 999;
            }

        }

        public class DataMigrationDependenciesModule1 : IDataMigration {
            public string Feature {
                get { return "Feature1"; }
            }

            public int Create() {
                return 999;
            }
        }
        public class DataMigrationDependenciesModule2 : IDataMigration {
            public string Feature {
                get { return "Feature2"; }
            }

            public int Create() {
                return 999;
            }
        }

        public class DataMigrationWithSchemaBuilder : DataMigrationImpl {
            public override string Feature {
                get { return "Feature1"; }
            }

            public int Create() {
                Assert.That(SchemaBuilder, Is.Not.Null);
                Assert.That(SchemaBuilder.TablePrefix, Is.EqualTo("TEST_"));
                return 1;
            }
        }
        [Test]
        public void DataMigrationShouldDoNothingIfNoDataMigrationIsProvidedForFeature() {
            Init(new Type[] {typeof (DataMigrationEmpty)});

            _folders.Manifests.Add("Module2", @"
name: Module2
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");

            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(0));
        }

        [Test]
        public void DataMigrationShouldDoNothingIfNoUpgradeOrCreateMethodWasFound() {
            Init(new Type[] { typeof(DataMigration11) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");
            
            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateShouldReturnVersionNumber() {
            Init(new Type[] { typeof(DataMigration11Create) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");
            
            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(1));
            Assert.That(_repository.Table.First().Current, Is.EqualTo(999));
            Assert.That(_repository.Table.First().DataMigrationClass, Is.EqualTo("Orchard.Tests.DataMigration.DataMigrationTests+DataMigration11Create"));
        }

        [Test]
        public void CreateCanBeFollowedByUpdates() {
            Init(new Type[] {typeof (DataMigrationCreateCanBeFollowedByUpdates)});

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");
            
            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(1));
            Assert.That(_repository.Table.First().Current, Is.EqualTo(666));
        }

        [Test]
        public void SameMigrationClassCanEvolve() {
            Init(new Type[] { typeof(DataMigrationSameMigrationClassCanEvolve) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");
            _repository.Create(new DataMigrationRecord() {
                Current = 42,
                DataMigrationClass = "Orchard.Tests.DataMigration.DataMigrationTests+DataMigrationSameMigrationClassCanEvolve"
            });

            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(1));
            Assert.That(_repository.Table.First().Current, Is.EqualTo(999));
        }

        [Test]
        public void DependenciesShouldBeUpgradedFirst() {

            Init(new Type[] { typeof(DataMigrationDependenciesModule1), typeof(DataMigrationDependenciesModule2) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
    Dependencies: Feature2
");

            _folders.Manifests.Add("Module2", @"
name: Module2
version: 0.1
orchardversion: 1
features:
  Feature2: 
    Description: Feature
");
            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(2));
            Assert.That(_repository.Fetch(d => d.Current == 999).Count(), Is.EqualTo(2));
            Assert.That(_repository.Fetch(d => d.DataMigrationClass == "Orchard.Tests.DataMigration.DataMigrationTests+DataMigrationDependenciesModule1").Count(), Is.EqualTo(1));
            Assert.That(_repository.Fetch(d => d.DataMigrationClass == "Orchard.Tests.DataMigration.DataMigrationTests+DataMigrationDependenciesModule2").Count(), Is.EqualTo(1));
        }

        [Test]
        public void DataMigrationImplShouldGetASchemaBuilder() {
            Init(new Type[] { typeof(DataMigrationWithSchemaBuilder) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");

            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(1));
        }
    }
}