using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.UI.Admin;

namespace Orchard.AuditTrail.Controllers {
    [Admin]
    public class ContentController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;

        public ContentController(IAuthorizer authorizer, IContentManager contentManager) {
            _authorizer = authorizer;
            _contentManager = contentManager;
        }

        public ActionResult Detail(int id, int version) {
            var contentItem = _contentManager.Get(id, VersionOptions.Number(version));
            if (!_authorizer.Authorize(Core.Contents.Permissions.ViewContent, contentItem))
                return new HttpUnauthorizedResult();

            var editor = _contentManager.BuildEditor(contentItem);
            return View(editor);
        }
    }
}