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
    public class TaxonomyController : Controller {
        private readonly IUpgradeService _upgradeService;
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;

        public TaxonomyController(
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
            ViewBag.CanMigrate = false; 

            if(_featureManager.GetEnabledFeatures().All(x => x.Id != "Orchard.Taxonomies")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.Taxonomies in order to migrate Contrib.Taxonomies to Orchard.Taxonomies."));
            }
            else {
                var flag = false;
                _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Taxonomies_TermContentItem"), (reader, conn) => {
                    flag = true;
                });

                if (flag) {
                    _orchardServices.Notifier.Warning(T("This migration step might have been done already."));
                }

                ViewBag.CanMigrate = true; 
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to migrate Contrib.Taxonomies.")))
                return new HttpUnauthorizedResult();
            try {
                _upgradeService.CopyTable("Contrib_Taxonomies_TaxonomyPartRecord", "Orchard_Taxonomies_TaxonomyPartRecord", new string[0]);
                _upgradeService.CopyTable("Contrib_Taxonomies_TermContentItem", "Orchard_Taxonomies_TermContentItem", new[] {"Id"});
                _upgradeService.CopyTable("Contrib_Taxonomies_TermPartRecord", "Orchard_Taxonomies_TermPartRecord", new string[0]);
                _upgradeService.CopyTable("Contrib_Taxonomies_TermsPartRecord", "Orchard_Taxonomies_TermsPartRecord", new string[0]);

                _orchardServices.Notifier.Information(T("Taxonomies were migrated successfully."));
            }
            catch(Exception e) {
                Logger.Error(e, "Unexpected error while migrating to Orchard.Taxonomies. Please check the log.");
                _orchardServices.Notifier.Error(T("Unexpected error while migrating to Orchard.Taxonomies. Please check the log."));

            }

            return RedirectToAction("Index");
        }
    }
}
