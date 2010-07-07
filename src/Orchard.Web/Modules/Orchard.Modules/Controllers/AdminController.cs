using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Modules.ViewModels;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;
        private readonly IDataMigrationManager _dataMigrationManager;

        public AdminController(IOrchardServices services, IModuleService moduleService, IDataMigrationManager dataMigrationManager) {
            Services = services;
            _moduleService = moduleService;
            _dataMigrationManager = dataMigrationManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            var modules = _moduleService.GetInstalledModules().ToList();
            return View(new ModulesIndexViewModel {Modules = modules});
        }

        public ActionResult Add() {
            return View(new ModuleAddViewModel());  
        }

        [HttpPost, ActionName("Add")]
        public ActionResult AddPOST() {
            // module not used for anything other than display (and that only to not have object in the view 'T')
            var viewModel = new ModuleAddViewModel();
            try {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Couldn't upload module package.")))
                    return new HttpUnauthorizedResult();

                if (string.IsNullOrWhiteSpace(Request.Files[0].FileName)) {
                    ModelState.AddModelError("File", T("Select a file to upload.").ToString());
                }

                if (!ModelState.IsValid)
                    return View("add", viewModel);

                foreach (string fileName in Request.Files) {
                    var file = Request.Files[fileName];
                    //todo: upload & process module package
                }

                //todo: add success message
                return RedirectToAction("index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Uploading module package failed: {0}",  exception.Message));
                return View("add", viewModel);
            }
        }

        public ActionResult Features() {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var features = _moduleService.GetAvailableFeatures().ToList();
            var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            return View(new FeaturesViewModel { Features = features, FeaturesThatNeedUpdate = featuresThatNeedUpdate });
        }

        [HttpPost]
        public ActionResult Enable(string id, bool? force) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return new NotFoundResult();

            _moduleService.EnableFeatures(new[] {id}, force != null && (bool) force);

            return RedirectToAction("Features");
        }

        [HttpPost]
        public ActionResult Disable(string id, bool? force) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return new NotFoundResult();

            _moduleService.DisableFeatures(new[] {id}, force != null && (bool) force);

            return RedirectToAction("Features");
        }

        [HttpPost]
        public ActionResult Update(string id, bool? force) {
            if ( !Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")) )
                return new HttpUnauthorizedResult();

            if ( string.IsNullOrEmpty(id) )
                return new NotFoundResult();

            try {
                _dataMigrationManager.Update(id);
                Services.Notifier.Information(T("The feature {0} was updated succesfuly", id));
            }
            catch(Exception ex) {
                Services.Notifier.Error(T("An error occured while updating the feature {0}: {1}", id, ex.Message));
            }

            return RedirectToAction("Features");
        }
    }
}