using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Contents.Controllers {
    [Themed]
    public class ItemController : Controller {
        private readonly IContentManager _contentManager;

        public ItemController(IContentManager contentManager, IShapeFactory shapeFactory, IOrchardServices services) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            Services = services;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        // /Contents/Item/Display/72
        public ActionResult Display(int? id, int? version) {
            if (id == null)
                return HttpNotFound();

            if (version.HasValue)
                return Preview(id, version);

            var contentItem = _contentManager.Get(id.Value, VersionOptions.Published);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.ViewContent, contentItem, T("Cannot view content"))) {
                return new HttpUnauthorizedResult();
            }

            var model = _contentManager.BuildDisplay(contentItem);
            return View(model);
        }

        // /Contents/Item/Preview/72
        // /Contents/Item/Preview/72?version=5
        public ActionResult Preview(int? id, int? version) {
            if (id == null)
                return HttpNotFound();

            var versionOptions = VersionOptions.Latest;

            if (version != null)
                versionOptions = VersionOptions.Number((int)version);

            var contentItem = _contentManager.Get(id.Value, versionOptions);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PreviewContent, contentItem, T("Cannot preview content"))) {
                return new HttpUnauthorizedResult();
            }

            var model = _contentManager.BuildDisplay(contentItem);
            return View(model);
        }
    }
}