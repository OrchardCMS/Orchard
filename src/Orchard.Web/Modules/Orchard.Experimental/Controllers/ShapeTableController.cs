using System.Web.Mvc;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Experimental.Controllers {
    [Themed, Admin]
    public class ShapeTableController : Controller {
        private readonly IShapeTableManager _shapeTableManager;

        public ShapeTableController(IShapeTableManager shapeTableManager) {
            _shapeTableManager = shapeTableManager;
        }

        public ActionResult ShapeTable(string themeName) {
            return View(_shapeTableManager.GetShapeTable(themeName));
        }
    }
}