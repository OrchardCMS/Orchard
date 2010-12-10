using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;

namespace Orchard.Packaging.Controllers {
    [OrchardFeature("PackagingServices")]
    [Themed, Admin]
    public class PackagingServicesController : Controller {

        private readonly IPackageManager _packageManager;
        private readonly IAppDataFolderRoot _appDataFolderRoot;
        private readonly INotifier _notifier;

        public PackagingServicesController(
            IPackageManager packageManager,
            INotifier notifier,
            IAppDataFolderRoot appDataFolderRoot,
            IOrchardServices services) {
            _packageManager = packageManager;
            _notifier = notifier;
            _appDataFolderRoot = appDataFolderRoot;
            Services = services;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult AddTheme(string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add themes")))
                return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost, ActionName("AddTheme")]
        public ActionResult AddThemePOST(string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add themes")))
                return new HttpUnauthorizedResult();

            return InstallPackage(returnUrl, Request.RawUrl);
        }

        [HttpPost, ActionName("RemoveTheme")]
        public ActionResult RemoveThemePOST(string themeId, string returnUrl, string retryUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to remove themes")))
                return new HttpUnauthorizedResult();

            return UninstallPackage(PackagingSourceManager.ThemesPrefix + themeId, returnUrl, retryUrl);
        }

        public ActionResult AddModule(string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add modules")))
                return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost, ActionName("AddModule")]
        public ActionResult AddModulePOST(string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add modules")))
                return new HttpUnauthorizedResult();

            return InstallPackage(returnUrl, Request.RawUrl);
        }

        public ActionResult InstallPackage(string returnUrl, string retryUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to install packages")))
                return new HttpUnauthorizedResult();

            try {
                if (Request.Files != null &&
                    Request.Files.Count > 0 &&
                    !string.IsNullOrWhiteSpace(Request.Files[0].FileName)) {
                    ModelState.AddModelError("File", T("Select a file to upload.").ToString());
                }

                foreach (string fileName in Request.Files) {
                    HttpPostedFileBase file = Request.Files[fileName];
                    if (file != null) {
                        string fullFileName = Path.Combine(_appDataFolderRoot.RootFolder, fileName + ".nupkg").Replace(Path.DirectorySeparatorChar, '/');
                        file.SaveAs(fullFileName);
                        PackageInfo info = _packageManager.Install(new ZipPackage(fullFileName), _appDataFolderRoot.RootFolder, HostingEnvironment.MapPath("~/"));
                        System.IO.File.Delete(fullFileName);

                        _notifier.Information(T("Installed package \"{0}\", version {1} of type \"{2}\" at location \"{3}\"",
                            info.ExtensionName, info.ExtensionVersion, info.ExtensionType, info.ExtensionPath));
                    }
                }

                return Redirect(returnUrl);
            } catch (Exception exception) {
                for (Exception scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(T("Uploading module package failed: {0}", exception.Message));
                }

                return Redirect(retryUrl);
            }
        }

        public ActionResult UninstallPackage(string id, string returnUrl, string retryUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to uninstall packages")))
                return new HttpUnauthorizedResult();

            try {
                _packageManager.Uninstall(id, HostingEnvironment.MapPath("~/"));

                _notifier.Information(T("Uninstalled package \"{0}\"", id));

                return Redirect(returnUrl);
            } catch (Exception exception) {
                for (Exception scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(T("Uninstall failed: {0}", exception.Message));
                }

                return Redirect(retryUrl);
            }
        }
    }
}