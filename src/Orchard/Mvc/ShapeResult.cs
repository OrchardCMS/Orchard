using System.Web.Mvc;

namespace Orchard.Mvc {
    public class ShapeResult : ViewResult {
        public ShapeResult(dynamic shape) {
            ViewData.Model = shape;
            ViewName = "~/Core/Shapes/Views/ShapeResult/Display.cshtml";
        }
    }
}
