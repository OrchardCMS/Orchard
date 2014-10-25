using System.Web.Mvc;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.AuditTrail.Controllers {
    [Admin]
    public class ContentController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentController(IAuthorizer authorizer, IContentManager contentManager, INotifier notifier, IContentDefinitionManager contentDefinitionManager) {
            _authorizer = authorizer;
            _contentManager = contentManager;
            _notifier = notifier;
            _contentDefinitionManager = contentDefinitionManager;
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
        public ActionResult Restore(int id, int version, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Number(version));
            if (!_authorizer.Authorize(Core.Contents.Permissions.PublishContent, contentItem))
                return new HttpUnauthorizedResult();

            var contentType = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            var draftable = contentType.Settings.GetModel<ContentTypeSettings>().Draftable;
            var restoredContentItem = _contentManager.Restore(contentItem, VersionOptions.Restore(version, publish: !draftable));
            var restoredContentItemTitle = _contentManager.GetItemMetadata(restoredContentItem).DisplayText;

            _notifier.Information(T("&quot;{0}&quot; has been restored.", restoredContentItemTitle));

            return this.RedirectReturn(returnUrl, () => Url.Action("Index", "Admin"));
        }
    }
}