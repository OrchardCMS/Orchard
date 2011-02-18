using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.PackageManager.Services;
using Orchard.PackageManager.ViewModels;
using Orchard.Packaging.Services;
using Orchard.Reports;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.PackageManager.Controllers {
    public class AdminController : Controller {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly INotifier _notifier;
        private readonly IPackageUpdateService _packageUpdateService;
        private readonly IBackgroundPackageUpdateStatus _backgroundPackageUpdateStatus;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IReportsManager _reportsManager;

        public AdminController(IOrchardServices services,
            IPackagingSourceManager packagingSourceManager,
            INotifier notifier,
            IPackageUpdateService packageUpdateService,
            IBackgroundPackageUpdateStatus backgroundPackageUpdateStatus,
            IReportsCoordinator reportsCoordinator,
            IReportsManager reportsManager) {

            _packagingSourceManager = packagingSourceManager;
            _notifier = notifier;
            _packageUpdateService = packageUpdateService;
            _backgroundPackageUpdateStatus = backgroundPackageUpdateStatus;
            _reportsCoordinator = reportsCoordinator;
            _reportsManager = reportsManager;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult ThemesUpdates(int? reportId) {
            return PackageUpdate("ThemesUpdate", DefaultExtensionTypes.Theme, reportId);
        }

        public ActionResult ModulesUpdates(int? reportId) {
            return PackageUpdate("ModulesUpdate", DefaultExtensionTypes.Theme, reportId);
        }

        private ActionResult PackageUpdate(string view, string extensionType, int? reportId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to install packages")))
                return new HttpUnauthorizedResult();

            if (reportId != null)
                CreateNotificationsFromReport(reportId.Value);

            if (!_packagingSourceManager.GetSources().Any()) {
                Services.Notifier.Error(T("No Gallery feed configured"));
                return View(view, new PackageList { Entries = new List<UpdatePackageEntry>() });
            }

            // Get status from background task state or directly
            _backgroundPackageUpdateStatus.Value =
                _backgroundPackageUpdateStatus.Value ??
                _packageUpdateService.GetPackagesStatus(_packagingSourceManager.GetSources());

            foreach (var error in _backgroundPackageUpdateStatus.Value.Errors) {
                for (var scan = error; scan != null; scan = scan.InnerException) {
                    Services.Notifier.Warning(T("Package retrieve error: {0}", scan.Message));
                }
            }

            return View(view, new PackageList {
                Entries = _backgroundPackageUpdateStatus.Value.Entries
                .Where(updatePackageEntry => updatePackageEntry.ExtensionsDescriptor.ExtensionType.Equals(extensionType))
            });
        }

        public ActionResult RefreshThemes() {
            _packageUpdateService.TriggerRefresh();
            _backgroundPackageUpdateStatus.Value = null;
            return RedirectToAction("ThemesUpdates");
        }

        public ActionResult RefreshModules() {
            _packageUpdateService.TriggerRefresh();
            _backgroundPackageUpdateStatus.Value = null;
            return RedirectToAction("ModulesUpdates");
        }

        public ActionResult Install(string packageId, string version, int sourceId, string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to install packages")))
                return new HttpUnauthorizedResult();

            _backgroundPackageUpdateStatus.Value =
                _backgroundPackageUpdateStatus.Value ??
                _packageUpdateService.GetPackagesStatus(_packagingSourceManager.GetSources());

            var entry = _backgroundPackageUpdateStatus.Value
                .Entries
                .SelectMany(e => e.PackageVersions)
                .Where(e => e.PackageId == packageId && e.Version == version && e.Source.Id == sourceId)
                .FirstOrDefault();
            if (entry == null) {
                return HttpNotFound();
            }

            try {
                _packageUpdateService.Update(entry);
            }
            catch (Exception exception) {
                Logger.Error(exception, "Error installing package {0}, version {1} from source {2}", packageId, version, sourceId);
                _notifier.Error(T("Error installing package update."));
                for (Exception scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(T("{0}", scan.Message));
                }
            }

            int reportId = CreateReport(T("Package Update"), T("Update of package {0} to version {1}", packageId, version));

            return RedirectToAction(returnUrl, new { reportId });
        }

        public ActionResult Uninstall(string packageId, string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to install packages")))
                return new HttpUnauthorizedResult();

            try {
                _packageUpdateService.Uninstall(packageId);
            }
            catch (Exception exception) {
                Logger.Error(exception, "Error un-installing package {0}", packageId);
                _notifier.Error(T("Error un-installing package."));
                for (Exception scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(T("{0}", scan.Message));
                }
            }

            int reportId = CreateReport(T("Package Uninstall"), T("Un-installation of package {0}", packageId));

            return RedirectToAction(returnUrl, new { reportId });
        }

        private void CreateNotificationsFromReport(int reportId) {
            // If we have notification in TempData, we don't need to display the
            // report as notifications (i.e. the AppDomain hasn't been restarted)
            // Note: This relies on an implementation detail of "Orchard.UI.Notify.NotifyFilter"
            if (TempData["messages"] != null)
                return;

            var report = _reportsManager.Get(reportId);
            if (report == null)
                return;

            if (report.Entries.Any()) {
                _notifier.Information(T("Application has been restarted. The following notifications originate from report #{0}:", reportId));
            }
            foreach(var entry in report.Entries) {
                switch(entry.Type) {
                    case ReportEntryType.Information:
                        _notifier.Add(NotifyType.Information, T(entry.Message));
                        break;
                    case ReportEntryType.Warning:
                        _notifier.Add(NotifyType.Warning, T(entry.Message));
                        break;
                    case ReportEntryType.Error:
                    default:
                        _notifier.Add(NotifyType.Error, T(entry.Message));
                        break;
                }
            }
        }

        private int CreateReport(LocalizedString activityName, LocalizedString title) {
            // Create a persistent report with all notifications, in case the application needs to be restarted
            const string reportKey = "PackageManager";

            int reportId = _reportsCoordinator.Register(reportKey, activityName.Text, title.Text);

            foreach(var notifyEntry in _notifier.List()) {
                switch (notifyEntry.Type) {
                    case NotifyType.Information:
                        _reportsCoordinator.Add(reportKey, ReportEntryType.Information, notifyEntry.Message.Text);
                        break;
                    case NotifyType.Warning:
                        _reportsCoordinator.Add(reportKey, ReportEntryType.Warning, notifyEntry.Message.Text);
                        break;
                    case NotifyType.Error:
                    default:
                        _reportsCoordinator.Add(reportKey, ReportEntryType.Error, notifyEntry.Message.Text);
                        break;
                }
            }

            return reportId;
        }
    }
}
