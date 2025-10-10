using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.ContentPreview.Controllers {
    [Themed]
    public class PreviewController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IClock _clock;
        private readonly IAuthorizer _authorizer;
        private readonly IHttpContextAccessor _hca;

        public PreviewController(
            IContentManager contentManager,
            INotifier notifier,
            IClock clock,
            IAuthorizer authorizer,
            IHttpContextAccessor hca) {
            _clock = clock;
            _notifier = notifier;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _hca = hca;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() => View();

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Render() {
            if (!_authorizer.Authorize(Permissions.ContentPreview)) {
                return new HttpUnauthorizedResult();
            }

            var contentItemType = _hca.Current().Request.Form["ContentItemType"];
            var contentItem = _contentManager.New(contentItemType);

            contentItem.VersionRecord = new ContentItemVersionRecord();

            var commonPart = contentItem.As<CommonPart>();

            if (commonPart != null) {
                commonPart.CreatedUtc = commonPart.ModifiedUtc = commonPart.PublishedUtc = _clock.UtcNow;
            }

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                return View();
            }

            _notifier.Warning(T("The Content Preview feature doesn't support properties where there are relationships to ContentPartRecord (e.g. Taxonomies, Tags). These won't update in the preview windows but otherwise keep working."));

            model = _contentManager.BuildDisplay(contentItem, "Detail");

            return View(model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) =>
            TryUpdateModel(model, prefix, includeProperties, excludeProperties);

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) =>
            ModelState.AddModelError(key, errorMessage.ToString());
    }
}