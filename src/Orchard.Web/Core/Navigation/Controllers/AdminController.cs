using System.Web.Mvc;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IOrchardServices _services;
        private readonly INavigationManager _navigationManager;

        public AdminController(IOrchardServices services, INavigationManager navigationManager) {
            _services = services;
            _navigationManager = navigationManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var model = new NavigationManagementViewModel {Menu = _navigationManager.BuildMenu("main")};

            return View(model);
        }
    }
}
