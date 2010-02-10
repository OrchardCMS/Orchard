using System;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Models;
using Orchard.Data.Migrations;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    public class SetupController : Controller {
        private readonly INotifier _notifier;
        private readonly IDatabaseMigrationManager _databaseMigrationManager;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsLoader _shellSettingsLoader;

        public SetupController(
            INotifier notifier,
            IDatabaseMigrationManager databaseMigrationManager,
            IOrchardHost orchardHost,
            IShellSettingsLoader shellSettingsLoader) {
            _notifier = notifier;
            _databaseMigrationManager = databaseMigrationManager;
            _orchardHost = orchardHost;
            _shellSettingsLoader = shellSettingsLoader;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult Index(SetupViewModel model) {
            return View(model ?? new SetupViewModel { AdminUsername = "admin" });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            try {
                if (!ModelState.IsValid) {
                    return Index(model);
                }

                var shellSettings = new ShellSettings {
                                                          Name = "default",
                                                          DataProvider = model.DatabaseOptions ? "SQLite" : "SqlServer",
                                                          DataConnectionString = model.DatabaseConnectionString
                                                      };

                // initialize the database:
                // provider: SqlServer or SQLite 
                // dataFolder: physical path (map before calling). Builtin database will be created in this location
                // connectionString: optional - if provided the dataFolder is essentially ignored, but should still be passed in
                _databaseMigrationManager.CreateCoordinator(shellSettings.DataProvider, Server.MapPath("~/App_Data"), shellSettings.DataConnectionString);

                // creating a standalone environment. 
                // in theory this environment can be used to resolve any normal components by interface, and those
                // components will exist entirely in isolation - no crossover between the safemode container currently in effect
                using (var finiteEnvironment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                    var contentManager = finiteEnvironment.Resolve<IContentManager>();

                    // create superuser
                    var membershipService = finiteEnvironment.Resolve<IMembershipService>();
                    var user = membershipService.CreateUser(new CreateUserParams(model.AdminUsername, model.AdminPassword, String.Empty, String.Empty, String.Empty, true));

                    // set site name and settings
                    var siteService = finiteEnvironment.Resolve<ISiteService>();
                    var siteSettings = siteService.GetSiteSettings().As<SiteSettings>();
                    siteSettings.Record.SiteSalt = Guid.NewGuid().ToString("N");
                    siteSettings.Record.SiteName = model.SiteName;
                    siteSettings.Record.SuperUser = model.AdminUsername;
                    siteSettings.Record.PageTitleSeparator = " - ";

                    // create home page as a CMS page
                    var page = contentManager.Create("page");
                    page.As<BodyAspect>().Text = "Welcome to Orchard";
                    page.As<RoutableAspect>().Slug = "";
                    page.As<RoutableAspect>().Title = model.SiteName;
                    page.As<HasComments>().CommentsShown = false;
                    page.As<CommonAspect>().Owner = user;
                    contentManager.Publish(page);

                    var authenticationService = finiteEnvironment.Resolve<IAuthenticationService>();
                    authenticationService.SignIn(user, true);
                }

                _shellSettingsLoader.SaveSettings(shellSettings);

                _orchardHost.Reinitialize();

                _notifier.Information(T("Setup succeeded"));

                // redirect to the welcome page.
                return Redirect("~/");
            }
            catch (Exception exception) {
                _notifier.Error(T("Setup failed: " + exception.Message));
                return Index(model);
            }
        }
    }
}
