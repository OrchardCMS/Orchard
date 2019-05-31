using System.Net;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
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


        public PreviewController(
            IContentManager contentManager,
            INotifier notifier,
            IClock clock,
            IAuthorizer authorizer) {
            _clock = clock;
            _notifier = notifier;
            _contentManager = contentManager;
            _authorizer = authorizer;
        }


        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Render() {
            if (!_authorizer.Authorize(Permissions.ContentPreview)) {
                return new HttpUnauthorizedResult();
            }

            var contentItemType = Request.Form["ContentItemType"];
            var contentItem = _contentManager.New(contentItemType);

            var commonPart = contentItem.As<CommonPart>();
            commonPart.CreatedUtc = commonPart.ModifiedUtc= commonPart.PublishedUtc = _clock.UtcNow;
            
            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            model = _contentManager.BuildDisplay(contentItem, "Detail");

            return View(model);
        }


        [ValidateInput(false)]
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}