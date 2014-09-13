using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;

namespace Orchard.Localization.Drivers {
    [UsedImplicitly]
    public class LocalizationPartDriver : ContentPartDriver<LocalizationPart> {
        private const string TemplatePrefix = "Localization";
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;

        public LocalizationPartDriver(ICultureManager cultureManager, ILocalizationService localizationService, IContentManager contentManager) {
            _cultureManager = cultureManager;
            _localizationService = localizationService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(LocalizationPart part, string displayType, dynamic shapeHelper) {
            var masterId = part.HasTranslationGroup
                               ? part.Record.MasterContentItemId
                               : part.Id;

            var siteCultures = _cultureManager.ListCultures();

            return Combined(
                ContentShape("Parts_Localization_ContentTranslations",
                             () => shapeHelper.Parts_Localization_ContentTranslations(Id: part.ContentItem.Id, MasterId: masterId, Culture: GetCulture(part), Localizations: GetDisplayLocalizations(part, VersionOptions.Published))),
                ContentShape("Parts_Localization_ContentTranslations_Summary",
                             () => shapeHelper.Parts_Localization_ContentTranslations_Summary(Id: part.ContentItem.Id, MasterId: masterId, Culture: GetCulture(part), Localizations: GetDisplayLocalizations(part, VersionOptions.Published))),
                ContentShape("Parts_Localization_ContentTranslations_SummaryAdmin",
                             () => shapeHelper.Parts_Localization_ContentTranslations_SummaryAdmin(Id: part.ContentItem.Id, MasterId: masterId, Culture: GetCulture(part), Localizations: GetDisplayLocalizations(part, VersionOptions.Latest), SiteCultures: siteCultures))
                );
        }

        protected override DriverResult Editor(LocalizationPart part, dynamic shapeHelper) {
            List<LocalizationPart> localizations = GetEditorLocalizations(part).ToList();

            var siteCultures = _cultureManager.ListCultures().ToList();

            List<string> missingCultures;
             
            if (part.HasTranslationGroup) {
                var localizationPart = part.MasterContentItem.As<LocalizationPart>();
                missingCultures =
                    siteCultures.Where(s => GetEditorLocalizations(localizationPart).All(l => l.Culture.Culture != s))
                        .ToList();

                missingCultures.Remove(localizationPart.Culture.Culture);
            }
            else {
                missingCultures =
                    siteCultures.Where(s => GetEditorLocalizations(part).All(l => l.Culture.Culture != s))
                        .ToList();

                if (part.Culture != null)
                    missingCultures.Remove(part.Culture.Culture);
            }

            var model = new EditLocalizationViewModel {
                SelectedCulture = GetCulture(part),
                SiteCultures = siteCultures,
                MissingCultures = missingCultures,
                ContentItem = part,
                MasterContentItem = part.HasTranslationGroup ? part.MasterContentItem : null,
                ContentLocalizations = new ContentLocalizationsViewModel(part) { Localizations = localizations }
            };

            return ContentShape("Parts_Localization_ContentTranslations_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Localization.ContentTranslations.Edit", Model: model, Prefix: TemplatePrefix));
        }

        protected override DriverResult Editor(LocalizationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new EditLocalizationViewModel();
            if (updater != null && updater.TryUpdateModel(model, TemplatePrefix, null, null)) {
                _localizationService.SetContentCulture(part, model.SelectedCulture);
            }

            return Editor(part, shapeHelper);
        }

        private static string GetCulture(LocalizationPart part) {
            return part.Culture != null ? part.Culture.Culture : null;
        }

        private IEnumerable<LocalizationPart> GetDisplayLocalizations(LocalizationPart part, VersionOptions versionOptions) {
            return _localizationService.GetLocalizations(part.ContentItem, versionOptions)
                .Select(c => {
                            var localized = c.ContentItem.As<LocalizationPart>();
                            if (localized.Culture == null)
                                localized.Culture = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture());
                            return c;
                        }).ToList();
        }

        private IEnumerable<LocalizationPart> GetEditorLocalizations(LocalizationPart part) {
            return _localizationService.GetLocalizations(part.ContentItem, VersionOptions.Latest)
                .Select(c => {
                    var localized = c.ContentItem.As<LocalizationPart>();
                    if (localized.Culture == null)
                        localized.Culture = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture());
                    return c;
                }).ToList();
        }
        
        protected override void Importing(LocalizationPart part, ContentManagement.Handlers.ImportContentContext context) {
            var masterContentItem = context.Attribute(part.PartDefinition.Name, "MasterContentItem");
            if (masterContentItem != null) {
                var contentItem = context.GetItemFromSession(masterContentItem);
                if (contentItem != null) {
                    part.MasterContentItem = contentItem;
                }
            }

            var culture = context.Attribute(part.PartDefinition.Name, "Culture");
            if (culture != null) {
                var targetCulture = _cultureManager.GetCultureByName(culture);
                // Add Culture.
                if (targetCulture == null && _cultureManager.IsValidCulture(culture)) {
                    _cultureManager.AddCulture(culture);
                    targetCulture = _cultureManager.GetCultureByName(culture);
                }
                part.Culture = targetCulture;
            }
        }

        protected override void Exporting(LocalizationPart part, ContentManagement.Handlers.ExportContentContext context) {
            if (part.MasterContentItem != null) {
                var masterContentItemIdentity = _contentManager.GetItemMetadata(part.MasterContentItem).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("MasterContentItem", masterContentItemIdentity.ToString());
            }

            if (part.Culture != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("Culture", part.Culture.Culture);
            }
        }
    }
}