using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Records;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.ContentManagement;
using Orchard.Data.Providers;

namespace Orchard.Tests.DataMigration {
    [TestFixture]
    public class DataMigrationTests {
        private IContainer _container;
        private StubFolders _folders;
        private IDataMigrationManager _dataMigrationManager;
        private IRepository<DataMigrationRecord> _repository;

        private ISessionFactory _sessionFactory;
        private ISession _session;

        [TestFixtureSetUp]
        public void CreateDb() {
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(DataMigrationRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord));
        }

        public void InitDb() {
            foreach ( var record in _repository.Fetch(m => true) ) {
                _repository.Delete(record);
            }
            _repository.Flush();
        }

        public void Init(IEnumerable<Type> dataMigrations) {
           
            var builder = new ContainerBuilder();
            _folders = new StubFolders();
            
            builder.RegisterInstance(new ShellSettings { DataTablePrefix = "TEST_"});
            
            builder.RegisterType<SqlServerDataServicesProvider>().As<IDataServicesProvider>();
            builder.RegisterType<DataServicesProviderFactory>().As<IDataServicesProviderFactory>();
            builder.RegisterType<NullInterpreter>().As<IDataMigrationInterpreter>();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<DataMigrationManager>().As<IDataMigrationManager>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();
            foreach(var type in dataMigrations) {
                builder.RegisterType(type).As<IDataMigration>();
            }
            _container = builder.Build();
            _container.Resolve<IExtensionManager>();
            _dataMigrationManager = _container.Resolve<IDataMigrationManager>();
            _repository = _container.Resolve<IRepository<DataMigrationRecord>>();

            InitDb();
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
            public Feature Feature {
                get { return new Feature() {Descriptor = new FeatureDescriptor() {Name = "Feature1"}}; }
            }
        }

        public class DataMigration11 : IDataMigration {
            public Feature Feature {
                get { return new Feature() {Descriptor = new FeatureDescriptor() {Name = "Feature1"}}; }
            }
        }

        public class DataMigration11Create : IDataMigration {
            public Feature Feature {
                get { return new Feature() {Descriptor = new FeatureDescriptor() {Name = "Feature1"}}; }
            }

            public int Create() {
                return 999;
            }
        }

        public class DataMigrationCreateCanBeFollowedByUpdates : IDataMigration {
            public Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature1" } }; }
            }

            public int Create() {
                return 42;
            }

            public int UpdateFrom42() {
                return 666;
            }
        }

        public class DataMigrationSameMigrationClassCanEvolve : IDataMigration {
            public Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature1" } }; }
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
            public Feature Feature {
                get { return new Feature() {Descriptor = new FeatureDescriptor() {Name = "Feature1"}}; }
            }

            public int Create() {
                return 999;
            }
        }

        public class DataMigrationDependenciesModule2 : IDataMigration {
            public Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature2" } }; }
            }

            public int Create() {
                return 999;
            }
        }

        public class DataMigrationWithSchemaBuilder : DataMigrationImpl {
            public override Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature1" } }; }
            }

            public int Create() {
                Assert.That(SchemaBuilder, Is.Not.Null);
                return 1;
            }
        }

        public class DataMigrationFeatureNeedUpdate1 : IDataMigration {
            public Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature1" } }; }
            }
        }

        public class DataMigrationFeatureNeedUpdate2 : IDataMigration {
            public Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature2" } }; }
            }

            public int Create() {
                return 999;
            }
        }

        public class DataMigrationFeatureNeedUpdate3 : IDataMigration {
            public Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature3" } }; }
            }

            public int Create() {
                return 999;
            }

            public int UpdateFrom42() {
                return 999;
            }
        }
        
        public class DataMigrationSimpleBuilder : DataMigrationImpl {
            public override Feature Feature {
                get { return new Feature() { Descriptor = new FeatureDescriptor() { Name = "Feature1" } }; }
            }

            public int Create() {
                SchemaBuilder.CreateTable("UserRecord", table => 
                    table.Column("Id", DbType.Int32, column => 
                        column.PrimaryKey()));

                return 1;
            }
        }

        [Test]
        public void DataMigrationShouldDoNothingIfNoDataMigrationIsProvidedForFeature() {
            Init(new[] {typeof (DataMigrationEmpty)});

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
            Init(new[] { typeof(DataMigration11) });

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
            Init(new[] { typeof(DataMigration11Create) });

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
            Init(new[] {typeof (DataMigrationCreateCanBeFollowedByUpdates)});

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
            Init(new[] { typeof(DataMigrationSameMigrationClassCanEvolve) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");
            _repository.Create(new DataMigrationRecord {
                Current = 42,
                DataMigrationClass = "Orchard.Tests.DataMigration.DataMigrationTests+DataMigrationSameMigrationClassCanEvolve"
            });

            _dataMigrationManager.Update("Feature1");
            Assert.That(_repository.Table.Count(), Is.EqualTo(1));
            Assert.That(_repository.Table.First().Current, Is.EqualTo(999));
        }

        [Test]
        public void DependenciesShouldBeUpgradedFirst() {

            Init(new[] { typeof(DataMigrationDependenciesModule1), typeof(DataMigrationDependenciesModule2) });

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
            Init(new[] { typeof(DataMigrationWithSchemaBuilder) });

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

        [Test]
        public void ShouldDetectFeaturesThatNeedUpdates() {

            Init(new[] { typeof(DataMigrationFeatureNeedUpdate1), typeof(DataMigrationFeatureNeedUpdate2), typeof(DataMigrationFeatureNeedUpdate3) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
  Feature2: 
    Description: Feature
  Feature3: 
    Description: Feature
  Feature4: 
    Description: Feature
");

            // even if there is a data migration class, as it is empty there should me no migration to do
            Assert.That(_dataMigrationManager.GetFeaturesThatNeedUpdate().Contains("Feature1"), Is.False);

            // there is no available class for this feature
            Assert.That(_dataMigrationManager.GetFeaturesThatNeedUpdate().Contains("Feature4"), Is.False);

            // there is a create method and no record in db, so let's create it
            Assert.That(_dataMigrationManager.GetFeaturesThatNeedUpdate().Contains("Feature2"), Is.True);

            // there is an UpdateFrom42 method, so it should be fired if Current == 42

            _repository.Create(new DataMigrationRecord {
                Current = 42,
                DataMigrationClass = "Orchard.Tests.DataMigration.DataMigrationTests+DataMigrationFeatureNeedUpdate3"
            });

            Assert.That(_dataMigrationManager.GetFeaturesThatNeedUpdate().Contains("Feature3"), Is.True);

            _repository.Delete(_repository.Fetch(m => m.Current == 42).First());
            _repository.Flush();

            _repository.Create(new DataMigrationRecord {
                Current = 43,
                DataMigrationClass = "Orchard.Tests.DataMigration.DataMigrationTests+DataMigrationFeatureNeedUpdate3"
            });

            Assert.That(_dataMigrationManager.GetFeaturesThatNeedUpdate().Contains("Feature3"), Is.False);
        }


        [Test] public void SchemaBuilderShouldCreateSql() {

            Init(new[] { typeof(DataMigrationSimpleBuilder) });

            _folders.Manifests.Add("Module1", @"
name: Module1
version: 0.1
orchardversion: 1
features:
  Feature1: 
    Description: Feature
");

            _dataMigrationManager.Update("Feature1");
        }

    }
}