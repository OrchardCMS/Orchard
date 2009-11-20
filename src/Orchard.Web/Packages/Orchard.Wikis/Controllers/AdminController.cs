using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Mvc.ViewModels;
using Orchard.Security;

namespace Orchard.Wikis.Controllers {
    public class AdminController : Controller {
        //private readonly IContentManager _contentManager;

        //public AdminController(IContentManager contentManager) {
        //    _contentManager = contentManager;
        //}

        //public ActionResult Index() {
        //    return View(new AdminViewModel());
        //}

        //public IUser CurrentUser { get; set; }

        //public ActionResult Create() {
        //    var page = _contentManager.New("wikipage");
        //    _contentManager.Create(page);
            
        //    return RedirectToAction("View", new{page.Id});
        //}

        //public ActionResult View(int id) {
        //    var page = _contentManager.Get(id).As<CommonPart>();
        //    return View(page);
        //}
    }
}
