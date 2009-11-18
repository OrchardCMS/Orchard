using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Mvc.ViewModels;
using Orchard.Security;

namespace Orchard.Wikis.Controllers {
    public class AdminController : Controller {
        private readonly IModelManager _modelManager;

        public AdminController(IModelManager modelManager) {
            _modelManager = modelManager;
        }

        public ActionResult Index() {
            return View(new AdminViewModel());
        }

        public IUser CurrentUser { get; set; }

        public ActionResult Create() {
            var page = _modelManager.New("wikipage");
            _modelManager.Create(page);
            
            return RedirectToAction("View", new{page.Id});
        }

        public ActionResult View(int id) {
            var page = _modelManager.Get(id).As<CommonModel>();
            return View(page);
        }
    }
}
