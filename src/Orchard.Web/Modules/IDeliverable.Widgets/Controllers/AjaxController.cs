using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Controllers
{
    [OrchardFeature("IDeliverable.Widgets.Ajax")]
    public class AjaxController : Controller
    {

        private readonly IContentManager _contentManager;
        private readonly IShapeDisplay _shapeDisplay;

        public AjaxController(IContentManager contentManager, IShapeDisplay shapeDisplay)
        {
            _contentManager = contentManager;
            _shapeDisplay = shapeDisplay;
        }

        public ActionResult Display(int id, string displayType = null)
        {
            var contentItem = _contentManager.Get(id);

            if (contentItem == null)
                return HttpNotFound();

            var output = RenderContentItem(contentItem, displayType);

            return Content(output, "text/html");
        }

        private string RenderContentItem(ContentItem contentItem, string displayType)
        {
            var shape = _contentManager.BuildDisplay(contentItem, displayType ?? "");
            shape.Ajaxified = true;
            return _shapeDisplay.Display(shape);
        }
    }
}