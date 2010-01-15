using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.Themes.ViewModels;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Themes.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeService _themeService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public AdminController(IThemeService themeService, IAuthorizer authorizer, INotifier notifier) {
            _themeService = themeService;
            _authorizer = authorizer;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                var themes = _themeService.GetInstalledThemes();
                var currentTheme = _themeService.GetSiteTheme();
                var model = new ThemesIndexViewModel { CurrentTheme = currentTheme, Themes = themes };
                return View(model);
            }
            catch (Exception exception) {
                _notifier.Error(T("Listing themes failed: " + exception.Message));
                return View(new ThemesIndexViewModel());
            }
        }

        [HttpPost]
        public ActionResult Activate(string themeName) {
            try {
                if (!_authorizer.Authorize(Permissions.SetSiteTheme, T("Couldn't set the current theme")))
                    return new HttpUnauthorizedResult();
                _themeService.SetSiteTheme(themeName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Activating theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        public ActionResult Install() {
            return View(new AdminViewModel());
        }

        [HttpPost]
        public ActionResult Install(FormCollection input) {
            try {
                if (!_authorizer.Authorize(Permissions.InstallUninstallTheme, T("Couldn't install theme")))
                    return new HttpUnauthorizedResult();
                foreach (string fileName in Request.Files) {
                    HttpPostedFileBase file = Request.Files[fileName];
                    _themeService.InstallTheme(file);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Installing theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Uninstall(string themeName) {
            try {
                if (!_authorizer.Authorize(Permissions.InstallUninstallTheme, T("Couldn't uninstall theme")))
                    return new HttpUnauthorizedResult();
                _themeService.UninstallTheme(themeName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Uninstalling theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }
    }
}
