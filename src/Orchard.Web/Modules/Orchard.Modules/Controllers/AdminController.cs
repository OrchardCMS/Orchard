using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Modules.Events;
using Orchard.Modules.Models;
using Orchard.Modules.Services;
using Orchard.Modules.ViewModels;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IExtensionDisplayEventHandler _extensionDisplayEventHandler;
        private readonly IModuleService _moduleService;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly ShellDescriptor _shellDescriptor;

        public AdminController(
            IEnumerable<IExtensionDisplayEventHandler> extensionDisplayEventHandlers,
            IOrchardServices services,
            IModuleService moduleService,
            IDataMigrationManager dataMigrationManager,
            IReportsCoordinator reportsCoordinator,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            ShellDescriptor shellDescriptor,
            IShapeFactory shapeFactory)
        {
            Services = services;
            _extensionDisplayEventHandler = extensionDisplayEventHandlers.FirstOrDefault();
            _moduleService = moduleService;
            _dataMigrationManager = dataMigrationManager;
            _reportsCoordinator = reportsCoordinator;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _shellDescriptor = shellDescriptor;
            Shape = shapeFactory;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Index(ModulesIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);

            IEnumerable<ModuleEntry> modules = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor => DefaultExtensionTypes.IsModule(extensionDescriptor.ExtensionType) &&
                                              (string.IsNullOrEmpty(options.SearchText) || extensionDescriptor.Name.ToLowerInvariant().Contains(options.SearchText.ToLowerInvariant())))
                .OrderBy(extensionDescriptor => extensionDescriptor.Name)
                .Select(extensionDescriptor => new ModuleEntry { Descriptor = extensionDescriptor });

            int totalItemCount = modules.Count();

            if (pager.PageSize != 0) {
                modules = modules.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize);
            }

            modules = modules.ToList();
            foreach (ModuleEntry moduleEntry in modules) {
                moduleEntry.IsRecentlyInstalled = _moduleService.IsRecentlyInstalled(moduleEntry.Descriptor);

                if (_extensionDisplayEventHandler != null) {
                    foreach (string notification in _extensionDisplayEventHandler.Displaying(moduleEntry.Descriptor, ControllerContext.RequestContext)) {
                        moduleEntry.Notifications.Add(notification);
                    }
                }
            }

            return View(new ModulesIndexViewModel {
                Modules = modules,
                InstallModules = _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null,
                Options = options,
                Pager = Shape.Pager(pager).TotalItemCount(totalItemCount)
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
                Services.Notifier.Error(T("An error occured while updating the feature {0}: {1}", id, exception.Message));
            }

            return RedirectToAction("Features");
        }
    }
}