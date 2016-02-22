using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Upgrade.Services;

namespace Upgrade.Controllers {
    [Admin]
    public class ContentPickerController : Controller {
        private readonly IUpgradeService _upgradeService;
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;

        public ContentPickerController(
            IUpgradeService upgradeService,
            IOrchardServices orchardServices,
            IFeatureManager featureManager) {
            _upgradeService = upgradeService;
            _orchardServices = orchardServices;
            _featureManager = featureManager;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (_featureManager.GetEnabledFeatures().All(x => x.Id != "Orchard.ContentPicker")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.ContentPicker in order to migrate Content Picker items to Orchard.ContentPicker."));
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to migrate Orchard.ContentPicker.")))
                return new HttpUnauthorizedResult();

            try {
                _upgradeService.ExecuteReader("DELETE FROM " + _upgradeService.GetPrefixedTableName("Orchard_ContentPicker_ContentMenuItemPartRecord"), null);
                _upgradeService.CopyTable("Navigation_ContentMenuItemPartRecord", "Orchard_ContentPicker_ContentMenuItemPartRecord", new string[0]);

                _orchardServices.Notifier.Success(T("Content Picker menu items were migrated successfully."));
            }
            catch(Exception e) {
                Logger.Error(e, "Unexpected error while migrating to Orchard.ContentPicker. Please check the log.");
                _orchardServices.Notifier.Error(T("Unexpected error while migrating to Orchard.ContentPicker. Please check the log."));

            }

            return RedirectToAction("Index");
        }
    }
}
