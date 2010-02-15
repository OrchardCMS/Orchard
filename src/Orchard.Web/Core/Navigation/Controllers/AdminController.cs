using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Utility;
using MenuItem=Orchard.Core.Navigation.Models.MenuItem;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly INavigationManager _navigationManager;

        public AdminController(IOrchardServices services, INavigationManager navigationManager) {
            _services = services;
            _navigationManager = navigationManager;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult Index() {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Not allowed to manage the main menu")))
                return new HttpUnauthorizedResult();

            var model = new NavigationManagementViewModel { Menu = _navigationManager.BuildMenu("main") };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Create(CreateMenuItemViewModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            var menuItem = _services.ContentManager.New<MenuItem>(MenuItemDriver.ContentType.Name);
            model.MenuItem = _services.ContentManager.UpdateEditorModel(menuItem, this);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return Index();
            }

            if (string.IsNullOrEmpty(menuItem.As<MenuPart>().MenuPosition))
                menuItem.As<MenuPart>().MenuPosition = Position.GetNext(_navigationManager.BuildMenu("main"));
            menuItem.As<MenuPart>().OnMainMenu = true;

            _services.ContentManager.Create(model.MenuItem.Item.ContentItem);

            return RedirectToAction("Index");
        }

        //[ValidateAntiForgeryTokenOrchard, ActionName("Delete")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeletePOST(int menuItemId)
        {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            //todo -> delete

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
