using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Themes;

namespace Orchard.Core.Contents.Controllers {
    [Themed]
    public class ItemController : Controller {
        private readonly IContentManager _contentManager;

        public ItemController(IContentManager contentManager, IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
 
        // /Contents/Item/Display/72
        public ActionResult Display(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Published);
            var model = _contentManager.BuildDisplay(contentItem);
            return View(model);
        }

        // /Contents/Item/Preview/72
        // /Contents/Item/Preview/72?version=5
        public ActionResult Preview(int id, int? version) {
            var versionOptions = VersionOptions.Latest;
            if (version != null)
                versionOptions = VersionOptions.Number((int)version);

            var contentItem = _contentManager.Get(id, versionOptions);
            var model = _contentManager.BuildDisplay(contentItem);
            return View("Display", model);
        }
    }
}