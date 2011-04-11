using System.Linq;
using Microsoft.WindowsAzure;
using NUnit.Framework;
using Orchard.Azure.Environment.Configuration;
using Orchard.Environment.Configuration;

namespace Orchard.Azure.Tests.Environment.Configuration {
    [TestFixture]
    public class AzureShellSettingsManagerTests : AzureVirtualEnvironmentTest {

        protected CloudStorageAccount DevAccount;
        protected IShellSettingsManager ShellSettingsManager;

        protected override void OnInit() {
            CloudStorageAccount.TryParse("UseDevelopmentStorage=true", out DevAccount);
            ShellSettingsManager = new AzureShellSettingsManager(DevAccount, new Moq.Mock<IShellSettingsManagerEventHandler>().Object);
        }

        [SetUp]
        public void Setup() {
            // ensure default container is empty before running any test
            DeleteAllBlobs(AzureShellSettingsManager.ContainerName, DevAccount);
        }

        [TearDown]
        public void TearDown() {
            // ensure default container is empty after running tests
            DeleteAllBlobs(AzureShellSettingsManager.ContainerName, DevAccount);
        }

        [Test]
        public void SingleSettingsFileShouldComeBackAsExpected() {

            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLCe", DataConnectionString = "something else" });

            var settings = ShellSettingsManager.LoadSettings().Single();
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.Name, Is.EqualTo("Default"));
            Assert.That(settings.DataProvider, Is.EqualTo("SQLCe"));
            Assert.That(settings.DataConnectionString, Is.EqualTo("something else"));
        }

        [Test]
        public void SettingsShouldBeOverwritable() {
            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLCe", DataConnectionString = "something else" });
            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLCe2", DataConnectionString = "something else2" });

            var settings = ShellSettingsManager.LoadSettings().Single();
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.Name, Is.EqualTo("Default"));
            Assert.That(settings.DataProvider, Is.EqualTo("SQLCe2"));
            Assert.That(settings.DataConnectionString, Is.EqualTo("something else2"));
        }

        [Test]
        public void MultipleFilesCanBeDetected() {

            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLCe", DataConnectionString = "something else" });
            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Another", DataProvider = "SQLCe2", DataConnectionString = "something else2" });

            var settings = ShellSettingsManager.LoadSettings();
            Assert.That(settings.Count(), Is.EqualTo(2));

            var def = settings.Single(x => x.Name == "Default");
            Assert.That(def.Name, Is.EqualTo("Default"));
            Assert.That(def.DataProvider, Is.EqualTo("SQLCe"));
            Assert.That(def.DataConnectionString, Is.EqualTo("something else"));

            var alt = settings.Single(x => x.Name == "Another");
            Assert.That(alt.Name, Is.EqualTo("Another"));
            Assert.That(alt.DataProvider, Is.EqualTo("SQLCe2"));
            Assert.That(alt.DataConnectionString, Is.EqualTo("something else2"));
        }

        [Test]
        public void NewSettingsCanBeStored() {
            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLite", DataConnectionString = "something else" });

            var foo = new ShellSettings { Name = "Foo", DataProvider = "Bar", DataConnectionString = "Quux" };

            Assert.That(ShellSettingsManager.LoadSettings().Count(), Is.EqualTo(1));
            ShellSettingsManager.SaveSettings(foo);
            Assert.That(ShellSettingsManager.LoadSettings().Count(), Is.EqualTo(2));

            foo = ShellSettingsManager.LoadSettings().Where(s => s.Name == "Foo").Single();
            Assert.That(foo.Name, Is.StringContaining("Foo"));
            Assert.That(foo.DataProvider, Is.StringContaining("Bar"));
            Assert.That(foo.DataConnectionString, Is.StringContaining("Quux"));
        }

        [Test]
        public void SettingsCanContainSeparatorChar() {
            ShellSettingsManager.SaveSettings(new ShellSettings { Name = "Default", DataProvider = "SQLite", DataConnectionString = "Server=tcp:tjyptm5sfc.database.windows.net;Database=orchard;User ID=foo@bar;Password=foo;Trusted_Connection=False;Encrypt=True;" });

            var settings = ShellSettingsManager.LoadSettings().Where(s => s.Name == "Default").Single();
            Assert.That(settings.DataConnectionString, Is.EqualTo("Server=tcp:tjyptm5sfc.database.windows.net;Database=orchard;User ID=foo@bar;Password=foo;Trusted_Connection=False;Encrypt=True;"));
        }
    }
}
