using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.DisplayManagement;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;
using Orchard.Mvc;
using Orchard.UI.Notify;

namespace Orchard.Localization.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;

        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            ICultureManager cultureManager,
            ILocalizationService localizationService,
            IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            _localizationService = localizationService;
            T = NullLocalizer.Instance;
            Services = orchardServices;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Translate(int id, string to) {
            var masterContentItem = _contentManager.Get(id, VersionOptions.Latest);
            if (masterContentItem == null)
                return HttpNotFound();

            var masterLocalizationPart = masterContentItem.As<LocalizationPart>();
            if (masterLocalizationPart == null)
                return HttpNotFound();

            // Check is current item stll exists, and redirect.
            var existingTranslation = _localizationService.GetLocalizedContentItem(masterContentItem, to);
            if (existingTranslation != null) {
                var existingTranslationMetadata = _contentManager.GetItemMetadata(existingTranslation);
                return RedirectToAction(
                    Convert.ToString(existingTranslationMetadata.EditorRouteValues["action"]),
                    existingTranslationMetadata.EditorRouteValues);
            }

            var contentItemTranslation = _contentManager.New(masterContentItem.ContentType);

            var contentItemTranslationPart = contentItemTranslation.As<LocalizationPart>();

            contentItemTranslationPart.MasterContentItem = masterContentItem;

            var content = _contentManager.BuildEditor(contentItemTranslation);
            
            return View(content);
        }

        [HttpPost, ActionName("Translate")]
        [FormValueRequired("submit.Save")]
        public ActionResult TranslatePOST(int id) {
            return TranslatePOST(id, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Translate")]
        [FormValueRequired("submit.Publish")]
        public ActionResult TranslateAndPublishPOST(int id) {
            return TranslatePOST(id, contentItem => Services.ContentManager.Publish(contentItem));
        }

        public ActionResult TranslatePOST(int id, Action<ContentItem> conditionallyPublish) {
            var masterContentItem = _contentManager.Get(id, VersionOptions.Latest);
            if (masterContentItem == null)
                return HttpNotFound();

            var masterLocalizationPart = masterContentItem.As<LocalizationPart>();
            if (masterLocalizationPart == null)
                return HttpNotFound();

            var model = new EditLocalizationViewModel();
            TryUpdateModel(model, "Localization");

            var existingTranslation = _localizationService.GetLocalizedContentItem(masterContentItem, model.SelectedCulture);
            if (existingTranslation != null) {
                var existingTranslationMetadata = _contentManager.GetItemMetadata(existingTranslation);
                return RedirectToAction(
                    Convert.ToString(existingTranslationMetadata.EditorRouteValues["action"]), 
                    existingTranslationMetadata.EditorRouteValues);
            }

            var contentItemTranslation = _contentManager.New(masterContentItem.ContentType);

            var contentItemTranslationPart = contentItemTranslation.As<LocalizationPart>();

            contentItemTranslationPart.MasterContentItem = masterContentItem;

            _contentManager.Create(contentItemTranslation, VersionOptions.Draft);

            var content = _contentManager.UpdateEditor(contentItemTranslation, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                return View(content);
            }

            conditionallyPublish(contentItemTranslation);

            Services.Notifier.Information(T("Created content item translation."));

            var metadata = _contentManager.GetItemMetadata(contentItemTranslation);
            return RedirectToAction(Convert.ToString(metadata.EditorRouteValues["action"]), metadata.EditorRouteValues);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}