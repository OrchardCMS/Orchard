using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Orchard.FileSystems.Dependencies;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.FileSystems.Dependencies {
    [TestFixture]
    public class DependenciesFolderTests {
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
                Name = "name",
                LoaderName = "test",
                VirtualPath = "~/bin"
            };
            
            dependenciesFolder.StoreDescriptors(new [] { d });
            var e = dependenciesFolder.LoadDescriptors();
            Assert.That(e, Has.Count.EqualTo(1));
            Assert.That(e.First().Name, Is.EqualTo("name"));
            Assert.That(e.First().LoaderName, Is.EqualTo("test"));
            Assert.That(e.First().VirtualPath, Is.EqualTo("~/bin"));
        }

        [Test]
        public void StoreDescriptorsShouldNoOpIfNoChanges() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            var d1 = new DependencyDescriptor {
                Name = "name1",
                LoaderName = "test1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                Name = "name2",
                LoaderName = "test2",
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
                Name = "name1",
                LoaderName = "test1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                Name = "name2",
                LoaderName = "test2",
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

        [Test]
        public void LoadDescriptorsShouldWorkAcrossInstances() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            var d1 = new DependencyDescriptor {
                Name = "name1",
                LoaderName = "test1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                Name = "name2",
                LoaderName = "test2",
                VirtualPath = "~/bin2"
            };

            dependenciesFolder.StoreDescriptors(new[] { d1, d2 });
            
            // Create a new instance over the same appDataFolder
            var dependenciesFolder2 = new DefaultDependenciesFolder(new StubCacheManager(), appDataFolder);

            // Ensure descriptors were persisted properly
            var result = dependenciesFolder2.LoadDescriptors();
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(p => p.Name), Has.Some.EqualTo("name1"));
            Assert.That(result.Select(p => p.Name), Has.Some.EqualTo("name2"));
        }
    }
}
