using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Orchard.Core.Common.Controllers {
    public class RoutableController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;

        public RoutableController(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        [HttpPost]
        public ActionResult Slugify(string contentType, int? id, int? containerId) {
            const string slug = "";
            ContentItem contentItem = null;

            if (string.IsNullOrEmpty(contentType))
                return Json(slug);

            if (id != null)
                contentItem = _contentManager.Get((int) id);
            
            if (contentItem == null)
                contentItem = _contentManager.New(contentType);

            if (containerId != null) {
                var containerItem = _contentManager.Get((int)containerId);
                contentItem.As<ICommonAspect>().Container = containerItem;
            }

            _contentManager.UpdateEditorModel(contentItem, this);

            return Json(contentItem.As<RoutableAspect>().Slug ?? slug);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}