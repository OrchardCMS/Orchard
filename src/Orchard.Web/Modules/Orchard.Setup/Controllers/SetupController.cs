using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Data.Migrations;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    public class SetupController : Controller {
        private readonly INotifier _notifier;
        private readonly IDatabaseMigrationManager _databaseMigrationManager;
        private readonly IOrchardHost _orchardHost;

        public SetupController(
            INotifier notifier,
            IDatabaseMigrationManager databaseMigrationManager,
            IOrchardHost orchardHost) {
            _notifier = notifier;
            _databaseMigrationManager = databaseMigrationManager;
            _orchardHost = orchardHost;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult Index() {
            return View(new SetupViewModel { AdminUsername = "admin" });
        }

        [HttpPost]
        public ActionResult Index(SetupViewModel model) {
            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                return View(model);
            }

            //notes: service call to initialize database:
            //_databaseMigrationManager.CreateCoordinator(provider, dataFolder, connectionString);
            // provider: SqlServer or SQLite 
            // dataFolder: physical path (map before calling). Builtin database will be created in this location
            // connectionString: optional - if provided the dataFolder is essentially ignored, but should still be passed in

            
            //notes: the other tool needed will be creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect
            var shellSettings = new ShellSettings { Name = "temp" };
            using (var finiteEnvironment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                var contentManager = finiteEnvironment.Resolve<IContentManager>();
                var yadda = contentManager.Create("yadda");

                // create superuser
                // set site name
                // database
                // redirect to the welcome page
            }


            _notifier.Information(T("Setup succeeded"));
            return RedirectToAction("Index");
        }
    }
}
