using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Services;
using Orchard.Localization;

namespace Orchard.Core.Common.Controllers {
    public class RoutableController : Controller, IUpdateModel {
        private readonly IRoutableService _routableService;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        public RoutableController(IRoutableService routableService, IContentManager contentManager, IOrchardServices orchardServices) {
            _routableService = routableService;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
        }

        [HttpPost]
        public ActionResult Slugify(FormCollection formCollection, string contentType) {
            var slug = "";

            if (string.IsNullOrEmpty(contentType))
                return Json(slug);

            var contentItem = _contentManager.New(contentType);
            _contentManager.UpdateEditorModel(contentItem, this);

            return Json(contentItem.As<RoutableAspect>().Slug);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}