using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Environment.Extensions {
    [TestFixture]
    public class ExtensionFoldersTests {
        private const string DataPrefix = "Orchard.Tests.Environment.Extensions.FoldersData.";
        private string _tempFolderName;

        [SetUp]
        public void Init() {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().Assembly;
            foreach (var name in assembly.GetManifestResourceNames()) {
                if (name.StartsWith(DataPrefix)) {
                    var text = "";
                    using (var stream = assembly.GetManifestResourceStream(name)) {
                        using (var reader = new StreamReader(stream))
                            text = reader.ReadToEnd();

                    }

                    var relativePath = name
                        .Substring(DataPrefix.Length)
                        .Replace(".txt", ":txt")
                        .Replace('.', Path.DirectorySeparatorChar)
                        .Replace(":txt", ".txt");

                    var targetPath = Path.Combine(_tempFolderName, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    using (var stream = new FileStream(targetPath, FileMode.Create)) {
                        using (var writer = new StreamWriter(stream)) {
                            writer.Write(text);
                        }
                    }
                }
            }
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolderName, true);
        }

        [Test]
        public void IdsFromFoldersWithModuleTxtShouldBeListed() {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(), new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] { _tempFolderName }, harvester);
            var ids = folders.AvailableExtensions().Select(d => d.Id);
            Assert.That(ids.Count(), Is.EqualTo(5));
            Assert.That(ids, Has.Some.EqualTo("Sample1")); // Sample1 - obviously
            Assert.That(ids, Has.Some.EqualTo("Sample3")); // Sample3
            Assert.That(ids, Has.Some.EqualTo("Sample4")); // Sample4
            Assert.That(ids, Has.Some.EqualTo("Sample6")); // Sample6
            Assert.That(ids, Has.Some.EqualTo("Sample7")); // Sample7
        }

        [Test]
        public void ModuleTxtShouldBeParsedAndReturnedAsYamlDocument() {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(), new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] { _tempFolderName }, harvester);
            var sample1 = folders.AvailableExtensions().Single(d => d.Id == "Sample1");
            Assert.That(sample1.Id, Is.Not.Empty);
            Assert.That(sample1.Author, Is.EqualTo("Bertrand Le Roy")); // Sample1
        }

        [Test]
        public void NamesFromFoldersWithModuleTxtShouldFallBackToIdIfNotGiven() {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(), new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] { _tempFolderName }, harvester);
            var names = folders.AvailableExtensions().Select(d => d.Name);
            Assert.That(names.Count(), Is.EqualTo(5));
            Assert.That(names, Has.Some.EqualTo("Le plug-in français")); // Sample1
            Assert.That(names, Has.Some.EqualTo("This is another test.txt")); // Sample3
            Assert.That(names, Has.Some.EqualTo("Sample4")); // Sample4
            Assert.That(names, Has.Some.EqualTo("SampleSix")); // Sample6
            Assert.That(names, Has.Some.EqualTo("Sample7")); // Sample7
        }

        [Test]
        public void PathsFromFoldersWithModuleTxtShouldFallBackAppropriatelyIfNotGiven() {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(), new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] { _tempFolderName }, harvester);
            var paths = folders.AvailableExtensions().Select(d => d.Path);
            Assert.That(paths.Count(), Is.EqualTo(5));
            Assert.That(paths, Has.Some.EqualTo("Sample1")); // Sample1 - Id, Name invalid URL segment
            Assert.That(paths, Has.Some.EqualTo("Sample3")); // Sample3 - Id, Name invalid URL segment
            Assert.That(paths, Has.Some.EqualTo("ThisIs.Sample4")); // Sample4 - Path
            Assert.That(paths, Has.Some.EqualTo("SampleSix")); // Sample6 - Name, no Path
            Assert.That(paths, Has.Some.EqualTo("Sample7")); // Sample7 - Id, no Name or Path
        }
    }
}