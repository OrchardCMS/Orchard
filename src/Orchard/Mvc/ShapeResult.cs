using System.Web.Mvc;

namespace Orchard.Mvc {
    public class ShapeResult : ViewResult {
        public ShapeResult(ControllerBase controller, dynamic shape) {
            ViewData = controller.ViewData;
            TempData = controller.TempData;
            ViewData.Model = shape;
            ViewName = "~/Core/Shapes/Views/ShapeResult/Display.cshtml";
        }
    }
}
