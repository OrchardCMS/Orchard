using System;
using System.Web.Mvc;
using Orchard.FileSystems.AppData;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    [ValidateInput(false)]
    public class SetupController : Controller {
        private readonly IAppDataFolder _appDataFolder;
        private readonly INotifier _notifier;
        private readonly ISetupService _setupService;

        public SetupController(INotifier notifier, ISetupService setupService, IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            _notifier = notifier;
            _setupService = setupService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            string message;
            if (!CanWriteToAppDataFolder(out message)) {
                _notifier.Error(
                    T(
                        "Hey, it looks like I can't write to the App_Data folder in the root of this application and that's where I need to save some of the information you're about to enter.\r\n\r\nPlease give me (the machine account this application is running under) write access to App_Data so I can get this app all set up for you.\r\n\r\nThanks!\r\n\r\n----\r\n{0}",
                        message));
            }

            return View(model);
        }

        public ActionResult Index() {
            var initialSettings = _setupService.Prime();
            return IndexViewResult(new SetupViewModel { AdminUsername = "admin", DatabaseIsPreconfigured = !string.IsNullOrEmpty(initialSettings.DataProvider)});
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            //HACK: (erikpo) Couldn't get a custom ValidationAttribute to validate two properties
            if (!model.DatabaseOptions && string.IsNullOrEmpty(model.DatabaseConnectionString))
                ModelState.AddModelError("DatabaseConnectionString", "A SQL connection string is required");

            if (!ModelState.IsValid) {
                return IndexViewResult(model);
            }

            try {

                var setupContext = new SetupContext {
                    SiteName = model.SiteName,
                    AdminUsername = model.AdminUsername,
                    AdminPassword = model.AdminPassword,
                    DatabaseProvider = model.DatabaseOptions ? "SQLite" : "SqlServer",
                    DatabaseConnectionString = model.DatabaseConnectionString,
                    DatabaseTablePrefix = model.DatabaseTablePrefix,
                    EnabledFeatures = null // default list
                };

                _setupService.Setup(setupContext);

                // redirect to the welcome page.
                return Redirect("~/");
            }
            catch (Exception exception) {
                _notifier.Error(T("Setup failed:"));
                for (var scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(new LocalizedString(scan.Message));
                }
                return IndexViewResult(model);
            }
        }

        bool CanWriteToAppDataFolder(out string message) {
            try {
                _appDataFolder.CreateFile("_systemcheck.txt", "Communicator check one two one two");
                _appDataFolder.DeleteFile("_systemcheck.txt");

                message = "";
                return true;
            }
            catch (Exception ex) {
                message = ex.Message.Replace("_systemcheck.txt", "");
                return false;
            }
        }
    }
}
