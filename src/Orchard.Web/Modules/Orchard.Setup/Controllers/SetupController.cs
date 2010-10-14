using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Linq;
using Orchard.FileSystems.AppData;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    [ValidateInput(false), Themed]
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

        private bool ValidateMachineKey() {
            // Get the machineKey section.
            var section = ConfigurationManager.GetSection("system.web/machineKey") as MachineKeySection;

            if (section == null
                || section.DecryptionKey.Contains("AutoGenerate")
                || section.ValidationKey.Contains("AutoGenerate")) {

                var rng = new RNGCryptoServiceProvider();
                var decryptionData = new byte[32];
                var validationData = new byte[64];
                
                rng.GetBytes(decryptionData);
                rng.GetBytes(validationData);

                string decryptionKey = BitConverter.ToString(decryptionData).Replace("-", "");
                string validationKey = BitConverter.ToString(validationData).Replace("-", "");

                ModelState.AddModelError("MachineKey", T("You need to define a MachineKey value in your web.config file. Here is one for you:\n <machineKey validationKey=\"{0}\" decryptionKey=\"{1}\" validation=\"SHA1\" decryption=\"AES\" />", validationKey, decryptionKey).ToString());
                return false;
            }

            return true;
        }

        public ActionResult Index() {
            ValidateMachineKey();

            var initialSettings = _setupService.Prime();
            return IndexViewResult(new SetupViewModel { AdminUsername = "admin", DatabaseIsPreconfigured = !string.IsNullOrEmpty(initialSettings.DataProvider)});
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            //HACK: (erikpo) Couldn't get a custom ValidationAttribute to validate two properties
            if (!model.DatabaseOptions && string.IsNullOrEmpty(model.DatabaseConnectionString))
                ModelState.AddModelError("DatabaseConnectionString", "A SQL connection string is required");

            if (!String.IsNullOrWhiteSpace(model.ConfirmPassword) && model.AdminPassword != model.ConfirmPassword ) {
                ModelState.AddModelError("ConfirmPassword", T("Password confirmation must match").ToString());
            }

            if(!model.DatabaseOptions && !String.IsNullOrWhiteSpace(model.DatabaseTablePrefix)) {
                model.DatabaseTablePrefix = model.DatabaseTablePrefix.Trim();
                if(!Char.IsLetter(model.DatabaseTablePrefix[0])) {
                    ModelState.AddModelError("DatabaseTablePrefix", T("The table prefix must begin with a letter").Text);
                }
            }

            ValidateMachineKey();

            if (!ModelState.IsValid) {
                return IndexViewResult(model);
            }

            try {

                var setupContext = new SetupContext {
                    SiteName = model.SiteName,
                    AdminUsername = model.AdminUsername,
                    AdminPassword = model.AdminPassword,
                    DatabaseProvider = model.DatabaseOptions ? "SqlCe" : "SqlServer",
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
