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
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            var lp = contentItem.As<LocalizationPart>();

            if (lp == null)
                return HttpNotFound();

            string contentItemCulture = _localizationService.GetContentCulture(contentItem);
            var localizations = _localizationService.GetLocalizations(lp, VersionOptions.Latest);
            
            var siteCultures = _cultureManager.ListCultures();

            var missingCultures = siteCultures.Where(s =>
                s != contentItemCulture
                && !localizations.Any(l => s == l.Culture.Culture))
                .ToList();

            string selectedCulture = null;

            if (!String.IsNullOrEmpty(to)) {

                if (!siteCultures.Any(c => String.Equals(c, to, StringComparison.OrdinalIgnoreCase)))
                    return HttpNotFound();

                var existingLocalization = String.Equals(contentItemCulture, to, StringComparison.OrdinalIgnoreCase)
                    ? lp
                    : localizations.FirstOrDefault(l => string.Equals(l.Culture.Culture, to, StringComparison.OrdinalIgnoreCase));

                if (existingLocalization != null) {
                    var metadata = _contentManager.GetItemMetadata(existingLocalization);
                    return RedirectToAction(Convert.ToString(metadata.EditorRouteValues["action"]), metadata.EditorRouteValues);
                }

                selectedCulture = missingCultures.SingleOrDefault(s => string.Equals(s, to, StringComparison.OrdinalIgnoreCase));
             }

            if (lp.Culture != null)
                lp.Culture.Culture = null;
            var model = new AddLocalizationViewModel {
                Id = id,
                SelectedCulture = selectedCulture,
                MissingCultures = missingCultures,
                Content = _contentManager.BuildEditor(contentItem)
            };

            // Cancel transaction so that the LocalizationPart is not modified.
            Services.TransactionManager.Cancel();

            return View(model);
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
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);
            var originalLp = contentItem.As<LocalizationPart>();

            if (contentItem == null)
                return HttpNotFound();

            var model = new AddLocalizationViewModel();
            TryUpdateModel(model);

            ContentItem contentItemTranslation;
            var existingTranslation = _localizationService.GetLocalizedContentItem(contentItem, model.SelectedCulture);
            if (existingTranslation != null) {
                // edit existing
                contentItemTranslation = _contentManager.Get(existingTranslation.ContentItem.Id, VersionOptions.DraftRequired);
            } else {
                // create
                contentItemTranslation = _contentManager.New(contentItem.ContentType);

                var translationLp = contentItemTranslation.As<LocalizationPart>();

                translationLp.MasterContentItem = originalLp.MasterContentItem ?? contentItem;

                if (!string.IsNullOrWhiteSpace(model.SelectedCulture))
                {
                    translationLp.Culture = _cultureManager.GetCultureByName(model.SelectedCulture);
                 }

                _contentManager.Create(contentItemTranslation, VersionOptions.Draft);
            }

            model.Content = _contentManager.UpdateEditor(contentItemTranslation, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                model.MissingCultures = _cultureManager.ListCultures().Where(s => s != _localizationService.GetContentCulture(contentItem) && s != _cultureManager.GetSiteCulture());

                var culture = originalLp.Culture;
                
                if (culture != null) {
                    culture.Culture = null;
                }
                model.Content = _contentManager.BuildEditor(contentItem);
                return View(model);
            }

            if (existingTranslation != null) {
                Services.Notifier.Information(T("Edited content item translation."));
            }
            else {
                conditionallyPublish(contentItemTranslation);

                Services.Notifier.Information(T("Created content item translation."));
            }

            var metadata = _contentManager.GetItemMetadata(model.Content.ContentItem);

            //todo: (heskew) if null, redirect to somewhere better than nowhere
            return metadata.EditorRouteValues == null ? null : RedirectToRoute(metadata.EditorRouteValues);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}