﻿using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.DisplayManagement;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using System;
using System.Web.Mvc;

namespace Orchard.Localization.Controllers
{
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;

        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            ILocalizationService localizationService,
            ICultureManager cultureManager,
            IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
            Services = orchardServices;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        [HttpPost]
        public ActionResult Translate(int id, string to) {
            var masterContentItem = _contentManager.Get(id, VersionOptions.Latest);
            if (masterContentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.ViewContent, masterContentItem, T("Couldn't open original content")))
                return new HttpUnauthorizedResult();

            var masterLocalizationPart = masterContentItem.As<LocalizationPart>();
            if (masterLocalizationPart == null)
                return HttpNotFound();

            // Check if current item still exists, and redirect.
            var existingTranslation = _localizationService.GetLocalizedContentItem(masterContentItem, to);
            if (existingTranslation != null) {
                var existingTranslationMetadata = _contentManager.GetItemMetadata(existingTranslation);
                return RedirectToAction(
                    Convert.ToString(existingTranslationMetadata.EditorRouteValues["action"]),
                    existingTranslationMetadata.EditorRouteValues);
            }

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(masterContentItem.ContentType);

            var contentItemTranslation = _contentManager.Clone(masterContentItem);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItemTranslation, T("Couldn't create translated content")))
                return new HttpUnauthorizedResult();

            var localizationPart = contentItemTranslation.As<LocalizationPart>();
            if(localizationPart != null) {
                localizationPart.MasterContentItem = masterContentItem;
                localizationPart.Culture = string.IsNullOrWhiteSpace(to) ? null : _cultureManager.GetCultureByName(to);
            }

            Services.Notifier.Success(T("Successfully cloned. The translated content was saved as a draft."));

            var adminRouteValues = _contentManager.GetItemMetadata(contentItemTranslation).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }
    }
}