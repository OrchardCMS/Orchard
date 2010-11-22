using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.Themes.Preview;
using Orchard.Themes.Services;
using Orchard.Themes.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Themes.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IPreviewTheme _previewTheme;
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IThemeService _themeService;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IReportsCoordinator _reportsCoordinator;

        public AdminController(
            IDataMigrationManager dataMigraitonManager,
            IReportsCoordinator reportsCoordinator,
            IOrchardServices services,
            IThemeManager themeManager,
            IFeatureManager featureManager,
            ISiteThemeService siteThemeService,
            IPreviewTheme previewTheme,
            IAuthorizer authorizer,
            INotifier notifier,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IThemeService themeService) {
            Services = services;
            _dataMigrationManager = dataMigraitonManager;
            _reportsCoordinator = reportsCoordinator;
            _siteThemeService = siteThemeService;
            _previewTheme = previewTheme;
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _themeService = themeService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                var currentTheme = _siteThemeService.GetSiteTheme();
                var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

                var themes = _extensionManager.AvailableExtensions()
                    .Where(d => d.ExtensionType == "Theme")
                    .Select(d => new ThemeEntry {
                        Descriptor = d,
                        NeedsUpdate = featuresThatNeedUpdate.Contains(d.Id),
                        Enabled = _shellDescriptor.Features.Any(sf => sf.Name == d.Id)
                    })
                    .ToArray();

                var model = new ThemesIndexViewModel { CurrentTheme = currentTheme, Themes = themes };
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
                _siteThemeService.SetSiteTheme(themeName);
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

                _themeService.EnableThemeFeatures(themeName);
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

                _themeService.DisableThemeFeatures(themeName);
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

                _themeService.EnableThemeFeatures(themeName);
                _siteThemeService.SetSiteTheme(themeName);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Activating theme failed: " + exception.Message));
            }
            return RedirectToAction("Index");
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