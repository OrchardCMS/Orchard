using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Modules.Services;
using Orchard.Modules.ViewModels;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly ShellDescriptor _shellDescriptor;

        public AdminController(IOrchardServices services,
            IModuleService moduleService,
            IDataMigrationManager dataMigrationManager,
            IReportsCoordinator reportsCoordinator,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            ShellDescriptor shellDescriptor)
        {
            Services = services;
            _moduleService = moduleService;
            _dataMigrationManager = dataMigrationManager;
            _reportsCoordinator = reportsCoordinator;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _shellDescriptor = shellDescriptor;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            IEnumerable<Module> modules = _extensionManager.AvailableExtensions()
                .Where(x => DefaultExtensionTypes.IsModule(x.ExtensionType))
                .Select(extensionDescriptor => new Module(extensionDescriptor) {
                                IsRecentlyInstalled = _moduleService.IsRecentlyInstalled(extensionDescriptor)
                            });

            return View(new ModulesIndexViewModel { 
                Modules = modules,
                InstallModules = _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null
            });
        }

        public ActionResult Features() {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            IEnumerable<ModuleFeature> features = _featureManager.GetAvailableFeatures()
                .Where(f => !DefaultExtensionTypes.IsTheme(f.Extension.ExtensionType))
                .Select(f => new ModuleFeature {
                                Descriptor = f,
                                IsEnabled = _shellDescriptor.Features.Any(sf => sf.Name == f.Id),
                                IsRecentlyInstalled = _moduleService.IsRecentlyInstalled(f.Extension),
                                NeedsUpdate = featuresThatNeedUpdate.Contains(f.Id)
                            });

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
                Services.Notifier.Information(T("The feature {0} was updated successfully", id));
            } catch (Exception exception) {
                this.Error(exception, T("An error occured while updating the feature {0}: {1}", id, exception.Message), Logger, Services.Notifier);
            }

            return RedirectToAction("Features");
        }
    }
}