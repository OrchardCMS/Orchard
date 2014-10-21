using System.Web.Mvc;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.AuditTrail.Controllers {
    [Admin]
    public class ContentController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;

        public ContentController(IAuthorizer authorizer, IContentManager contentManager, INotifier notifier) {
            _authorizer = authorizer;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Detail(int id, int version) {
            var contentItem = _contentManager.Get(id, VersionOptions.Number(version));
            if (!_authorizer.Authorize(Core.Contents.Permissions.ViewContent, contentItem))
                return new HttpUnauthorizedResult();

            var auditTrailPart = contentItem.As<AuditTrailPart>();

            if (auditTrailPart != null) {
                auditTrailPart.ShowComment = true;
            }

            var editor = _contentManager.BuildEditor(contentItem);
            return View(editor);
        }

        [HttpPost]
        public ActionResult Rollback(int id, int version, string returnUrl) {
            var contentItem = _contentManager.Get(id);
            if (!_authorizer.Authorize(Core.Contents.Permissions.PublishContent, contentItem))
                return new HttpUnauthorizedResult();

            var title = _contentManager.GetItemMetadata(contentItem).DisplayText;
            var currentVersion = contentItem.Version;
            var newContentItem = _contentManager.Rollback(contentItem, VersionOptions.Rollback(version, publish: true));

            _notifier.Information(T("{0} has been rolled back from version {1} to version {2} as version {3}.", title, currentVersion, version, newContentItem.Version));

            returnUrl = Url.IsLocalUrl(returnUrl) 
                ? returnUrl
                : Request.UrlReferrer != null
                    ? Request.UrlReferrer.ToString()
                    : Url.Action("Index", "Admin");

            return Redirect(returnUrl);
        }
    }
}