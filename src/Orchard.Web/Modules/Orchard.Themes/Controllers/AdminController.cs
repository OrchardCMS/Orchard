using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.Themes.Preview;
using Orchard.Themes.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Themes.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeService _themeService;
        private readonly IPreviewTheme _previewTheme;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IReportsCoordinator _reportsCoordinator;

        public AdminController(
            IDataMigrationManager dataMigraitonManager,
            IReportsCoordinator reportsCoordinator,
            IOrchardServices services,
            IThemeService themeService,
            IPreviewTheme previewTheme,
            IAuthorizer authorizer,
            INotifier notifier,
            IShapeHelperFactory shapeHelperFactory) {
            Services = services;
            _dataMigrationManager = dataMigraitonManager;
            _reportsCoordinator = reportsCoordinator;
            _themeService = themeService;
            _previewTheme = previewTheme;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services{ get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                var themes = _themeService.GetInstalledThemes();
                var currentTheme = _themeService.GetSiteTheme();
                var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();
                var model = new ThemesIndexViewModel { CurrentTheme = currentTheme, Themes = themes, FeaturesThatNeedUpdate = featuresThatNeedUpdate };
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Listing themes failed: " + exception.Message));
                return View(new ThemesIndexViewModel());
            }
        }

        [HttpPost, FormValueAbsent("submit.Apply"), FormValueAbsent("submit.Cancel")]
        public ActionResult Preview(string themeName, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();
                _previewTheme.SetPreviewTheme(themeName);
                return Redirect(returnUrl ?? "~/");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Previewing theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Apply")]
        public ActionResult ApplyPreview(string themeName, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();
                _previewTheme.SetPreviewTheme(null); 
                _themeService.SetSiteTheme(themeName);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Previewing theme failed: " + exception.Message));
            }
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Cancel")]
        public ActionResult CancelPreview(string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();
                _previewTheme.SetPreviewTheme(null);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Previewing theme failed: " + exception.Message));
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't enable the theme")))
                    return new HttpUnauthorizedResult();
                _themeService.EnableTheme(themeName);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Enabling theme failed: " + exception.Message));
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't disable the current theme")))
                    return new HttpUnauthorizedResult();
                _themeService.DisableTheme(themeName);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Disabling theme failed: " + exception.Message));
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Activate(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't set the current theme")))
                    return new HttpUnauthorizedResult();
                _themeService.SetSiteTheme(themeName);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Activating theme failed: " + exception.Message));
            }
            return RedirectToAction("Index");
        }

        public ActionResult Install() {
            return View();
        }

        [HttpPost]
        public ActionResult Install(FormCollection input) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageThemes, T("Couldn't install theme")))
                    return new HttpUnauthorizedResult();
                foreach (string fileName in Request.Files) {
                    HttpPostedFileBase file = Request.Files[fileName];
                    _themeService.InstallTheme(file);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Installing theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Uninstall(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageThemes, T("Couldn't uninstall theme")))
                    return new HttpUnauthorizedResult();
                _themeService.UninstallTheme(themeName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Uninstalling theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Update(string themeName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageThemes, T("Couldn't update theme")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(themeName))
                return HttpNotFound();

            try {
                _reportsCoordinator.Register("Data Migration", "Upgrade " + themeName, "Orchard installation");
                _dataMigrationManager.Update(themeName);
                Services.Notifier.Information(T("The theme {0} was updated succesfuly", themeName));
            }
            catch (Exception ex) {
                Services.Notifier.Error(T("An error occured while updating the theme {0}: {1}", themeName, ex.Message));
            }

            return RedirectToAction("Index");
        }

        class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueRequiredAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return !string.IsNullOrEmpty(value);
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class FormValueAbsentAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueAbsentAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return string.IsNullOrEmpty(value);
            }
        }
    }
}