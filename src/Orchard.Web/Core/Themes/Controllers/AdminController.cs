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
            return View(new ThemesIndexViewModel());
        }
    }
}
