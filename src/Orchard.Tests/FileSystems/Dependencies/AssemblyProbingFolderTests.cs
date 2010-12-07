using System.Linq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.FileSystems.Dependencies;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.FileSystems.Dependencies {
    [TestFixture]
    public class AssemblyProbingFolderTests {

        [Test]
        public void FolderShouldBeEmptyByDefault() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder, new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.That(dependenciesFolder.AssemblyExists("foo"), Is.False);
        }

        [Test]
        public void LoadAssemblyShouldNotThrowIfAssemblyNotFound() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder, new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.That(dependenciesFolder.LoadAssembly("foo"), Is.Null);
        }

        [Test]
        public void GetAssemblyDateTimeUtcShouldThrowIfAssemblyNotFound() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder, new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.That(() => dependenciesFolder.GetAssemblyDateTimeUtc("foo"), Throws.Exception);
        }

        [Test]
        public void DeleteAssemblyShouldNotThrowIfAssemblyNotFound() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder, new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.DoesNotThrow(() => dependenciesFolder.DeleteAssembly("foo"));
        }

        [Test]
        public void StoreAssemblyShouldCopyFile() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);

            var assembly = GetType().Assembly;
            var name = assembly.GetName().Name;

            {
                var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder, new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));
                dependenciesFolder.StoreAssembly(name, assembly.Location);
            }

            {
                var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder, new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));
                Assert.That(dependenciesFolder.AssemblyExists(name), Is.True);
                Assert.That(dependenciesFolder.LoadAssembly(name), Is.SameAs(GetType().Assembly));
                Assert.DoesNotThrow(() => dependenciesFolder.DeleteAssembly(name));
                Assert.That(dependenciesFolder.LoadAssembly(name), Is.Null);
            }
        }
    }
}
