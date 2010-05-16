using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.Environment.FileSystems;

namespace Orchard.Tests.Environment.Configuration {
    [TestFixture]
    public class DefaultTenantManagerTests {
        private string _tempFolder;
        private AppDataFolder _appData;

        [SetUp]
        public void Init() {
            _appData = new AppDataFolder();
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            _appData.SetBasePath(_tempFolder);
        }
        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        [Test]
        public void SingleSettingsFileShouldComeBackAsExpected() {
            
            _appData.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nDataProvider: SQLite\r\nDataConnectionString: something else");

            IShellSettingsManager loader = new ShellSettingsManager(_appData, new Mock<IShellSettingsManagerEventHandler>().Object);
            var settings = loader.LoadSettings().Single();
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.Name, Is.EqualTo("Default"));
            Assert.That(settings.DataProvider, Is.EqualTo("SQLite"));
            Assert.That(settings.DataConnectionString, Is.EqualTo("something else"));
        }


        [Test]
        public void MultipleFilesCanBeDetected() {

            _appData.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nDataProvider: SQLite\r\nDataConnectionString: something else");
            _appData.CreateFile("Sites\\Another\\Settings.txt", "Name: Another\r\nDataProvider: SQLite2\r\nDataConnectionString: something else2");

            IShellSettingsManager loader = new ShellSettingsManager(_appData, new Mock<IShellSettingsManagerEventHandler>().Object);
            var settings = loader.LoadSettings();
            Assert.That(settings.Count(), Is.EqualTo(2));

            var def = settings.Single(x => x.Name == "Default");
            Assert.That(def.Name, Is.EqualTo("Default"));
            Assert.That(def.DataProvider, Is.EqualTo("SQLite"));
            Assert.That(def.DataConnectionString, Is.EqualTo("something else"));

            var alt = settings.Single(x => x.Name == "Another");
            Assert.That(alt.Name, Is.EqualTo("Another"));
            Assert.That(alt.DataProvider, Is.EqualTo("SQLite2"));
            Assert.That(alt.DataConnectionString, Is.EqualTo("something else2"));
        }

        [Test]
        public void NewSettingsCanBeStored() {
            _appData.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nDataProvider: SQLite\r\nDataConnectionString: something else");

            IShellSettingsManager loader = new ShellSettingsManager(_appData, new Mock<IShellSettingsManagerEventHandler>().Object);
            var foo = new ShellSettings {Name = "Foo", DataProvider = "Bar", DataConnectionString = "Quux"};

            Assert.That(loader.LoadSettings().Count(), Is.EqualTo(1));
            loader.SaveSettings(foo);
            Assert.That(loader.LoadSettings().Count(), Is.EqualTo(2));

            var text = File.ReadAllText(_appData.MapPath("Sites\\Foo\\Settings.txt"));
            Assert.That(text, Is.StringContaining("Foo"));
            Assert.That(text, Is.StringContaining("Bar"));
            Assert.That(text, Is.StringContaining("Quux"));
        }
    }
}
