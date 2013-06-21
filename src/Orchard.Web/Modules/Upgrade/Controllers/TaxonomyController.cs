using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Upgrade.ViewModels;

namespace Upgrade.Controllers {
    [Admin]
    public class TaxonomyController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;

        public TaxonomyController(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            IFeatureManager featureManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _featureManager = featureManager;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if(!_featureManager.GetEnabledFeatures().Any(x => x.Id == "Orchard.Taxonomies")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.Taxonomies in order to migrate Contrib.Taxonomies to Orchard.Taxonomies."));
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to migrate Contrib.Taxonomies.")))
                return new HttpUnauthorizedResult();

            _orchardServices.Notifier.Information(T("Taxonomies were migrated successfully."));
            
            return RedirectToAction("Index");
        }
    }
}
