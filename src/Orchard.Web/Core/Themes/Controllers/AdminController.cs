using System;
using System.Web.Mvc;
using Orchard.Core.Themes.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Core.Themes.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeService _themeService;
        private readonly INotifier _notifier;

        public AdminController(IThemeService themeService, INotifier notifier) {
            _themeService = themeService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                var themes = _themeService.GetInstalledThemes();
                var currentTheme = _themeService.GetCurrentTheme();
                var model = new ThemesIndexViewModel { CurrentTheme = currentTheme, Themes = themes };
                return View(model);
            }
            catch (Exception exception) {
                _notifier.Error(T("Listing themes failed: " + exception.Message));
                return View(new ThemesIndexViewModel());
            }
        }

        public ActionResult Activate(string themeName) {
            try {
                _themeService.SetCurrentTheme(themeName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Activating theme failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }
    }
}
