using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.Services;
using Orchard.Core.Localization.ViewModels;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.UI.Notify;

namespace Orchard.Core.Localization.Controllers {
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
            IShapeHelperFactory shapeHelperFactory) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            _localizationService = localizationService;
            T = NullLocalizer.Instance;
            Services = orchardServices;
            Shape = shapeHelperFactory.CreateHelper();
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
            if (contentItem.Has<RoutePart>()) {
                var routePart = contentItem.As<RoutePart>();
                routePart.Slug = string.Format("{0}{2}{1}", routePart.Slug, siteCultures.Any(s => string.Equals(s, selectedCulture, StringComparison.OrdinalIgnoreCase)) ? selectedCulture : "", !string.IsNullOrWhiteSpace(routePart.Slug) ? "-" : "");
                routePart.Path = null;
            }

            if (contentItem.As<LocalizationPart>().Culture != null)
                contentItem.As<LocalizationPart>().Culture.Culture = null;
            var model = new AddLocalizationViewModel {
                Id = id,
                SelectedCulture = selectedCulture,
                SiteCultures = siteCultures,
                Content = _contentManager.BuildEditor(contentItem)
            };
            Services.TransactionManager.Cancel();

            return View(model);
        }

        [HttpPost, ActionName("Translate")]
        public ActionResult TranslatePOST(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            var model = new AddLocalizationViewModel();
            TryUpdateModel(model);

            ContentItem contentItemTranslation = null;
            var existingTranslation = _localizationService.GetLocalizedContentItem(contentItem, model.SelectedCulture);
            if (existingTranslation != null) {
                // edit existing
                contentItemTranslation = _contentManager.Get(existingTranslation.ContentItem.Id, VersionOptions.DraftRequired);
            }
            else {
                // create
                contentItemTranslation = _contentManager.New(contentItem.ContentType);
                var localized = contentItemTranslation.As<LocalizationPart>();
                localized.MasterContentItem = contentItem;
                if (!string.IsNullOrWhiteSpace(model.SelectedCulture))
                    localized.Culture = _cultureManager.GetCultureByName(model.SelectedCulture);
                _contentManager.Create(contentItemTranslation, VersionOptions.Draft);

                if (!contentItem.Has<IPublishingControlAspect>() && contentItem.VersionRecord != null && contentItem.VersionRecord.Published) {
                    _contentManager.Publish(contentItemTranslation);
                }
            }

            model.Content = _contentManager.UpdateEditor(contentItemTranslation, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                model.SiteCultures = _cultureManager.ListCultures().Where(s => s != _localizationService.GetContentCulture(contentItem) && s != _cultureManager.GetSiteCulture());
                contentItem.As<LocalizationPart>().Culture.Culture = null;
                model.Content = _contentManager.BuildEditor(contentItem);
                return View(model);
            }

            Services.Notifier.Information(T("Created content item translation."));

            var metadata = _contentManager.GetItemMetadata(model.Content);
            if (metadata.EditorRouteValues == null)
                return null; //todo: (heskew) redirect to somewhere better than nowhere

            return RedirectToRoute(metadata.EditorRouteValues);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}