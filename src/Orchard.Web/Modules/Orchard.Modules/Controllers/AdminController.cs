using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Modules.Services;
using Orchard.Modules.ViewModels;
using Orchard.Packaging.Services;
using Orchard.Reports.Services;
using Orchard.UI.Notify;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IPackageManager _packageManager;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly ShellDescriptor _shellDescriptor;

        public AdminController(IOrchardServices services,
            IModuleService moduleService,
            IDataMigrationManager dataMigrationManager,
            IPackageManager packageManager,
            IReportsCoordinator reportsCoordinator,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            ShellDescriptor shellDescriptor) {

            Services = services;
            _moduleService = moduleService;
            _dataMigrationManager = dataMigrationManager;
            _packageManager = packageManager;
            _reportsCoordinator = reportsCoordinator;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _shellDescriptor = shellDescriptor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            var modules = _extensionManager.AvailableExtensions().Where(x => x.ExtensionType == "Module");
            return View(new ModulesIndexViewModel { Modules = modules });
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
#if REFACTORING
                    var info = _packageManager.Install(file.InputStream);

                    Services.Notifier.Information(T("Installed package \"{0}\", version {1} of type \"{2}\" at location \"{3}\"",
                        info.ExtensionName, info.ExtensionVersion, info.ExtensionType, info.ExtensionPath));
#endif
                    }

                return RedirectToAction("index");
            }
            catch (Exception exception) {
                for (var scan = exception; scan != null; scan = scan.InnerException) {
                    Services.Notifier.Error(T("Uploading module package failed: {0}", exception.Message));
                }
                return View("add", viewModel);
            }
        }

        public ActionResult Features() {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            var features = _featureManager.GetAvailableFeatures()
                .Where(f => !f.Extension.ExtensionType.Equals("Theme", StringComparison.OrdinalIgnoreCase))
                .Select(f=>new ModuleFeature{Descriptor=f,
                IsEnabled=_shellDescriptor.Features.Any(sf=>sf.Name==f.Id),
                NeedsUpdate=featuresThatNeedUpdate.Contains(f.Id)})
                .ToList();

            return View(new FeaturesViewModel { Features = features });
        }

        [HttpPost]
        public ActionResult Enable(string id, bool? force) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            _moduleService.EnableFeatures(new[] { id }, force != null && (bool)force);

            return RedirectToAction("Features");
        }

        [HttpPost]
        public ActionResult Disable(string id, bool? force) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            _moduleService.DisableFeatures(new[] { id }, force != null && (bool)force);

            return RedirectToAction("Features");
        }

        [HttpPost]
        public ActionResult Update(string id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            try {
                _reportsCoordinator.Register("Data Migration", "Upgrade " + id, "Orchard installation");
                _dataMigrationManager.Update(id);
                Services.Notifier.Information(T("The feature {0} was updated succesfuly", id));
            }
            catch (Exception ex) {
                Services.Notifier.Error(T("An error occured while updating the feature {0}: {1}", id, ex.Message));
            }

            return RedirectToAction("Features");
        }
    }
}