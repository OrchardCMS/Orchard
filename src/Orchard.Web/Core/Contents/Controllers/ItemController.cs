using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Contents.Controllers {
    [Themed]
    public class ItemController : ContentControllerBase {
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _hca;

        public ItemController(
            IOrchardServices orchardServices,
            IHttpContextAccessor hca) : base(orchardServices.ContentManager) {
            _contentManager = orchardServices.ContentManager;
            _hca = hca;
            Services = orchardServices;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        // /Contents/Item/Display/72
        public ActionResult Display(int? id, int? version) {
            if (id == null)
                return HttpNotFound();

            if (version.HasValue)
                return Preview(id, version);

            var contentItem = _contentManager.Get(id.Value, VersionOptions.Published);

            var customRouteRedirection = GetCustomContentItemRouteRedirection(contentItem, ContentItemRoute.Display);
            if (customRouteRedirection != null) {
                return customRouteRedirection;
            }

            if (contentItem == null)
                return HttpNotFound();


            var container = contentItem.As<CommonPart>()?.Container;
            if (container != null && !container.HasPublished()) {
                // if the content has a container that has not a published version we check preview permissions
                // in order to check if user can view the content or not.
                // Open point: should we handle hierarchies? 
                if (!Services.Authorizer.Authorize(Permissions.PreviewContent, contentItem)) {
                    return HttpNotFound();
                }
            }

            if (!Services.Authorizer.Authorize(Permissions.ViewContent, contentItem, T("Cannot view content"))) {
                return new HttpUnauthorizedResult();
            }

            var model = _contentManager.BuildDisplay(contentItem);
            if (_hca.Current().Request.IsAjaxRequest()) {
                return new ShapePartialResult(this, model);
            }

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
            if (_hca.Current().Request.IsAjaxRequest()) {
                return new ShapePartialResult(this, model);
            }

            return View(model);
        }
    }
}