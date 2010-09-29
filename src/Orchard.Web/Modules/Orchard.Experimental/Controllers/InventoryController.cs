using System.Web.Mvc;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Experimental.Controllers {
    [Themed, Admin]
    public class InventoryController : Controller {
        private readonly IShapeTableManager _shapeTableManager;

        public InventoryController(IShapeTableManager shapeTableManager) {
            _shapeTableManager = shapeTableManager;
        }

        public ActionResult ShapeTable(string themeName) {
            return View("ShapeTable", _shapeTableManager.GetShapeTable(themeName));
        }
    }
}