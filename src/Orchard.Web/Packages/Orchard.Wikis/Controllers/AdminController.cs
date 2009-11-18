using System.Web.Mvc;
using Orchard.Mvc.ViewModels;

namespace Orchard.Wikis.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View(new AdminViewModel());
        }
    }
}
