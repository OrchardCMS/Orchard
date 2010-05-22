using System.Linq;
using Microsoft.WindowsAzure;
using NUnit.Framework;
using Orchard.Azure.Environment.Configuration;
using Orchard.Environment.Configuration;

namespace Orchard.Azure.Tests.Environment.Configuration {
    [TestFixture]
    public class AzureShellSettingsManagerTests : AzureVirtualEnvironmentTest {

        protected IShellSettingsManager Loader;

        protected override void OnInit() {
            CloudStorageAccount devAccount;
            CloudStorageAccount.TryParse("UseDevelopmentStorage=true", out devAccount);

            Loader = new AzureShellSettingsManager(devAccount, new Moq.Mock<IShellSettingsManagerEventHandler>().Object);
        }

        [SetUp]
        public void Setup() {
            // ensure default container is empty before running any test
            DeleteAllBlobs( ((AzureShellSettingsManager)Loader).Container);
        }

        [TearDown]
        public void TearDown() {
            // ensure default container is empty after running tests
            DeleteAllBlobs(( (AzureShellSettingsManager)Loader ).Container);
        }

        [Test]
        public void SingleSettingsFileShouldComeBackAsExpected() {

            Loader.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLite", DataConnectionString = "something else" });

            var settings = Loader.LoadSettings().Single();
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.Name, Is.EqualTo("Default"));
            Assert.That(settings.DataProvider, Is.EqualTo("SQLite"));
            Assert.That(settings.DataConnectionString, Is.EqualTo("something else"));
        }


        [Test]
        public void MultipleFilesCanBeDetected() {

            Loader.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLite", DataConnectionString = "something else" });
            Loader.SaveSettings(new ShellSettings { Name = "Another", DataProvider = "SQLite2", DataConnectionString = "something else2" });

            var settings = Loader.LoadSettings();
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
            Loader.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLite", DataConnectionString = "something else" });

            var foo = new ShellSettings { Name = "Foo", DataProvider = "Bar", DataConnectionString = "Quux" };

            Assert.That(Loader.LoadSettings().Count(), Is.EqualTo(1));
            Loader.SaveSettings(foo);
            Assert.That(Loader.LoadSettings().Count(), Is.EqualTo(2));

            var text = ( (AzureShellSettingsManager)Loader ).Container.GetBlockBlobReference("Foo/Settings.txt").DownloadText();
            Assert.That(text, Is.StringContaining("Foo"));
            Assert.That(text, Is.StringContaining("Bar"));
            Assert.That(text, Is.StringContaining("Quux"));
        }
    }
}
