using System.IO;
using System.Linq;
using NUnit.Framework;
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
        public void NamesFromFoldersWithModuleTxtShouldBeListed() {
            IExtensionFolders folders = new ModuleFolders(new[] { _tempFolderName }, new StubCacheManager(), new StubWebSiteFolder());
            var names = folders.AvailableExtensions().Select(d => d.Id);
            Assert.That(names.Count(), Is.EqualTo(2));
            Assert.That(names, Has.Some.EqualTo("Sample1"));
            Assert.That(names, Has.Some.EqualTo("Sample3"));
        }

        [Test]
        public void ModuleTxtShouldBeParsedAndReturnedAsYamlDocument() {
            IExtensionFolders folders = new ModuleFolders(new[] { _tempFolderName }, new StubCacheManager(), new StubWebSiteFolder());
            var sample1 = folders.AvailableExtensions().Single(d => d.Id == "Sample1");
            Assert.That(sample1.Id, Is.Not.Empty);
            Assert.That(sample1.Author, Is.EqualTo("Bertrand Le Roy"));
        }
   }
}