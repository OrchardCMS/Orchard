using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.DisplayManagement;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;
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

            // only support translations from the site culture, at the moment at least
            if (contentItem == null)
                return HttpNotFound();

            if (!contentItem.Is<LocalizationPart>() || contentItem.As<LocalizationPart>().MasterContentItem != null) {
                var metadata = _contentManager.GetItemMetadata(contentItem);
                return RedirectToAction(Convert.ToString(metadata.EditorRouteValues["action"]), metadata.EditorRouteValues);
            }
            
            var siteCultures = _cultureManager.ListCultures().Where(s => s != _localizationService.GetContentCulture(contentItem) && s != _cultureManager.GetSiteCulture());
            var selectedCulture = siteCultures.SingleOrDefault(s => string.Equals(s, to, StringComparison.OrdinalIgnoreCase))
                ?? _cultureManager.GetCurrentCulture(HttpContext); // could be null but the person doing the translating might be translating into their current culture

            //todo: need a better solution for modifying some parts when translating - or go with a completely different experience
            /*
            if (contentItem.Has<RoutePart>()) {
                RoutePart routePart = contentItem.As<RoutePart>();
                routePart.Slug = string.Format("{0}{2}{1}", routePart.Slug, siteCultures.Any(s => string.Equals(s, selectedCulture, StringComparison.OrdinalIgnoreCase)) ? selectedCulture : "", !string.IsNullOrWhiteSpace(routePart.Slug) ? "-" : "");
                routePart.Path = null;
            }*/

            if (contentItem.As<LocalizationPart>().Culture != null)
                contentItem.As<LocalizationPart>().Culture.Culture = null;
            var model = new AddLocalizationViewModel {
                Id = id,
                SelectedCulture = selectedCulture,
                SiteCultures = siteCultures,
                Content = _contentManager.BuildEditor(contentItem)
            };

            // Cancel transaction so that the routepart is not modified.
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
                if (contentItemTranslation.Has<ICommonPart>() && contentItem.Has<ICommonPart>()) {
                    contentItemTranslation.As<ICommonPart>().Container = contentItem.As<ICommonPart>().Container;
                }

                _contentManager.Create(contentItemTranslation, VersionOptions.Draft);
            }

            model.Content = _contentManager.UpdateEditor(contentItemTranslation, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                model.SiteCultures = _cultureManager.ListCultures().Where(s => s != _localizationService.GetContentCulture(contentItem) && s != _cultureManager.GetSiteCulture());
                var culture = contentItem.As<LocalizationPart>().Culture;
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
                LocalizationPart localized = contentItemTranslation.As<LocalizationPart>();
                localized.MasterContentItem = contentItem;
                if (!string.IsNullOrWhiteSpace(model.SelectedCulture)) {
                    localized.Culture = _cultureManager.GetCultureByName(model.SelectedCulture);
                }

                conditionallyPublish(contentItemTranslation);

                Services.Notifier.Information(T("Created content item translation."));
            }

            var metadata = _contentManager.GetItemMetadata(model.Content);

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