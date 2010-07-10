using System;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Contents.ViewModels;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Localization.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;

        public AdminController(IOrchardServices orchardServices, IContentManager contentManager) {
            _contentManager = contentManager;
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }

        public ActionResult Translate(int id, string from) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return new NotFoundResult();

            var model = new EditItemViewModel {
                Id = id,
                Content = _contentManager.BuildEditorModel(contentItem)
            };

            PrepareEditorViewModel(model.Content);
            return View(model);
        }

        [HttpPost, ActionName("Translate")]
        public ActionResult TranslatePOST(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.DraftRequired);

            if (contentItem == null)
                return new NotFoundResult();

            var viewModel = new EditItemViewModel();
            if (TryUpdateModel(viewModel))
                viewModel.Content = _contentManager.UpdateEditorModel(contentItem, this);

            //todo: create translation here
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                PrepareEditorViewModel(viewModel.Content);
                return View(viewModel);
            }
            _contentManager.Publish(contentItem);

            var metadata = _contentManager.GetItemMetadata(viewModel.Content.Item);
            if (metadata.EditorRouteValues == null)
                return null;

            return RedirectToRoute(metadata.EditorRouteValues);
        }

        private static void PrepareEditorViewModel(ContentItemViewModel itemViewModel) {
            if (string.IsNullOrEmpty(itemViewModel.TemplateName)) {
                itemViewModel.TemplateName = "Items/Contents.Item";
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}