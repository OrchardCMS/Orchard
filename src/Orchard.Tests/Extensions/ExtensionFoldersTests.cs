using System.IO;
using System.Linq;
using NUnit.Framework;
using Orchard.Extensions;
using Yaml.Grammar;

namespace Orchard.Tests.Extensions {
    [TestFixture]
    public class ExtensionFoldersTests {
        private const string DataPrefix = "Orchard.Tests.Extensions.FoldersData.";
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
            var folders = new ModuleFolders(new[] { _tempFolderName });
            var names = folders.ListNames();
            Assert.That(names.Count(), Is.EqualTo(2));
            Assert.That(names, Has.Some.EqualTo("Sample1"));
            Assert.That(names, Has.Some.EqualTo("Sample3"));
        }

        [Test]
        public void ModuleTxtShouldBeParsedAndReturnedAsYamlDocument() {
            var folders = new ModuleFolders(new[] { _tempFolderName });
            var sample1 = folders.ParseManifest("Sample1");
            var mapping = (Mapping)sample1.YamlDocument.Root;
            var entities = mapping.Entities
                .Where(x => x.Key is Scalar)
                .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);
            Assert.That(entities.Keys, Has.Some.EqualTo("name"));
            Assert.That(entities.Keys, Has.Some.EqualTo("author"));
        }
    }
}
