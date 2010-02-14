using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Orchard.Data.Migrations;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Setup.Controllers;
using Orchard.Setup.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Tests.Modules.Setup {
    [TestFixture, Ignore("this can't be made to work")]
    public class SetupControllerTests {
        private string _tempFolder;
        private IContainer _container;

        [SetUp]
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            Directory.CreateDirectory(_tempFolder);

            var hostContainer = OrchardStarter.CreateHostContainer(builder => {
                builder.Register(new ControllerBuilder());
                builder.Register(new ViewEngineCollection { new WebFormViewEngine() });
                builder.Register(new RouteCollection());
                builder.Register(new ModelBinderDictionary());
            });

            hostContainer.Resolve<IAppDataFolder>().SetBasePath(_tempFolder);

            var host = (DefaultOrchardHost)hostContainer.Resolve<IOrchardHost>();
            _container = host.CreateShellContainer();
            _container.Build(builder => {
                builder.Register<SetupController>();
            });


            //var builder = new ContainerBuilder();
            //builder.Register<SetupController>();
            //builder.Register<Notifier>().As<INotifier>();
            //builder.Register<DefaultOrchardHost>().As<IOrchardHost>();
            //builder.Register<DatabaseMigrationManager>().As<IDatabaseMigrationManager>();
            //builder.Register<ShellSettingsLoader>().As<IShellSettingsLoader>();
            //builder.Register<TestAppDataFolder>().As<IAppDataFolder>();
            //_container = builder.Build();
        }

        private string GetMessages() {
            var notifier = _container.Resolve<INotifier>();
            return notifier.List().Aggregate("", (a, b) => a + b.Message.ToString());
        }

        private SetupViewModel GetTestSetupModel() {
            return new SetupViewModel {
                AdminUsername = "test1",
                AdminPassword = "test2",
                DatabaseOptions = true,
                SiteName = "test3"
            };
        }

        [Test]
        public void IndexNormallyReturnsWithDefaultAdminUsername() {
            var controller = _container.Resolve<SetupController>();
            var result = controller.Index(null);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewResult>());

            var viewResult = (ViewResult)result;
            Assert.That(viewResult.ViewData.Model, Is.TypeOf<SetupViewModel>());

            var model2 = (SetupViewModel)viewResult.ViewData.Model;
            Assert.That(model2.AdminUsername, Is.EqualTo("admin"));
        }

        [Test]
        public void SetupShouldCreateShellSettingsFile() {
            var model = GetTestSetupModel();
            var controller = _container.Resolve<SetupController>();
            var result = controller.IndexPOST(model);

            Assert.That(GetMessages(), Is.StringContaining("Setup succeeded"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<RedirectResult>());
            Assert.That(File.Exists(Path.Combine(_tempFolder, "Sites\\default.txt")));
        }

        [Test]
        public void BuiltinDatabaseShouldCreateSQLiteFile() {
            var model = GetTestSetupModel();
            var controller = _container.Resolve<SetupController>();
            var result = controller.IndexPOST(model);

            Assert.That(GetMessages(), Is.StringContaining("Setup succeeded"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<RedirectResult>());
            Assert.That(File.Exists(Path.Combine(_tempFolder, "Sites\\default\\orchard.db")));
        }


    }
}
