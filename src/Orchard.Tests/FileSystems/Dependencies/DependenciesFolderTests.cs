using System;
using System.IO;
using NUnit.Framework;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.Dependencies;
using Orchard.Tests.FileSystems.AppData;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.FileSystems.Dependencies {
    [TestFixture]
    public class DependenciesFolderTests {

        [SetUp]
        public void Init() {
        }

        [TearDown]
        public void Term() {
        }

        [Test]
        public void LoadDescriptorsShouldReturnEmptyList() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            var e = dependenciesFolder.LoadDescriptors();
            Assert.That(e, Is.Empty);
        }

        [Test]
        public void StoreDescriptorsShouldWork() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            var d = new DependencyDescriptor {
                LoaderName = "test",
                Name = "name",
                VirtualPath = "~/bin"
            };
            
            dependenciesFolder.StoreDescriptors(new [] { d });
            var e = dependenciesFolder.LoadDescriptors();
            Assert.That(e, Has.Count.EqualTo(1));
        }

        [Test]
        public void StoreDescriptorsShouldNoOpIfNoChanges() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            var d1 = new DependencyDescriptor {
                LoaderName = "test1",
                Name = "name1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                LoaderName = "test2",
                Name = "name2",
                VirtualPath = "~/bin2"
            };

            dependenciesFolder.StoreDescriptors(new[] { d1, d2 });
            var dateTime1 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.xml"));
            clock.Advance(TimeSpan.FromMinutes(1));

            dependenciesFolder.StoreDescriptors(new[] { d2, d1 });
            var dateTime2 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.xml"));
            Assert.That(dateTime1, Is.EqualTo(dateTime2));
        }

        [Test]
        public void StoreDescriptorsShouldStoreIfChanges() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            var d1 = new DependencyDescriptor {
                LoaderName = "test1",
                Name = "name1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                LoaderName = "test2",
                Name = "name2",
                VirtualPath = "~/bin2"
            };

            dependenciesFolder.StoreDescriptors(new[] { d1, d2 });
            var dateTime1 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.xml"));
            clock.Advance(TimeSpan.FromMinutes(1));

            d1.LoaderName = "bar";

            dependenciesFolder.StoreDescriptors(new[] { d2, d1 });
            var dateTime2 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.xml"));
            Assert.That(dateTime1 + TimeSpan.FromMinutes(1), Is.EqualTo(dateTime2));
        }
    }
}
