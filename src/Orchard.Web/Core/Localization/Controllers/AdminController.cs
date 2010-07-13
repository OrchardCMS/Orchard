using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.Services;
using Orchard.Core.Localization.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc.Results;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Core.Localization.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;

        public AdminController(IOrchardServices orchardServices, IContentManager contentManager, ICultureManager cultureManager, ILocalizationService localizationService) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            _localizationService = localizationService;
            T = NullLocalizer.Instance;
            Services = orchardServices;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Translate(int id, string to) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return new NotFoundResult();

            var siteCultures = _cultureManager.ListCultures().Where(s => s != _localizationService.GetContentCulture(contentItem));
            var model = new AddLocalizationViewModel {
                Id = id,
                SelectedCulture = siteCultures.Any(s => s == to)
                    ? to
                    : _cultureManager.GetCurrentCulture(HttpContext), // could be null but the person doing the translating might be translating into their current culture
                SiteCultures = siteCultures,
                Content = _contentManager.BuildEditorModel(contentItem)
            };

            PrepareEditorViewModel(model.Content);
            return View(model);
        }

        [HttpPost, ActionName("Translate")]
        public ActionResult TranslatePOST(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return new NotFoundResult();

            var viewModel = new AddLocalizationViewModel();
            TryUpdateModel(viewModel);

            ContentItem contentItemTranslation;
            var existingTranslation = _localizationService.GetLocalizedContentItem(contentItem, viewModel.SelectedCulture);
            if (existingTranslation != null) { // edit existing
                contentItemTranslation = _contentManager.Get(existingTranslation.ContentItem.Id, VersionOptions.DraftRequired);
            }
            else { // create
                contentItemTranslation = _contentManager.New(contentItem.ContentType);
                var localized = contentItemTranslation.As<Localized>();
                localized.MasterContentItem = contentItem;
                localized.Culture = _cultureManager.GetCultureByName(viewModel.SelectedCulture);
                _contentManager.Create(contentItemTranslation, VersionOptions.Draft);
            }

            if (ModelState.IsValid)
                viewModel.Content = _contentManager.UpdateEditorModel(contentItemTranslation, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                viewModel.SiteCultures = _cultureManager.ListCultures().Where(s => s != _localizationService.GetContentCulture(contentItem));
                PrepareEditorViewModel(viewModel.Content);
                return View(viewModel);
            }

            var metadata = _contentManager.GetItemMetadata(viewModel.Content.Item);
            if (metadata.EditorRouteValues == null)
                return null; //todo: (heskew) redirect to somewhere better than nowhere

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