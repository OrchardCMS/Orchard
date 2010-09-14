using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes.Preview;
using Orchard.Themes.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Themes.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeService _themeService;
        private readonly IPreviewTheme _previewTheme;

        public AdminController(
            IOrchardServices services,
            IThemeService themeService,
            PreviewTheme previewTheme,
            IAuthorizer authorizer,
            INotifier notifier,
            IShapeHelperFactory shapeHelperFactory) {
            Services = services;
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
                var model = new ThemesIndexViewModel { CurrentTheme = currentTheme, Themes = themes };
                return View(Shape.Model(model));
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Listing themes failed: " + exception.Message));
                return View(Shape.Model(new ThemesIndexViewModel()));
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
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Previewing theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Cancel")]
        public ActionResult CancelPreview(string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();
                _previewTheme.SetPreviewTheme(null);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Previewing theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Activate(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't set the current theme")))
                    return new HttpUnauthorizedResult();
                _themeService.SetSiteTheme(themeName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Activating theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
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