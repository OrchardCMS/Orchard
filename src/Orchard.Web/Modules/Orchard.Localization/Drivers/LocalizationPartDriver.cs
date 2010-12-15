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

        public LocalizationPartDriver(ICultureManager cultureManager, ILocalizationService localizationService) {
            _cultureManager = cultureManager;
            _localizationService = localizationService;
        }

        protected override DriverResult Display(LocalizationPart part, string displayType, dynamic shapeHelper) {
            var masterId = part.MasterContentItem != null
                               ? part.MasterContentItem.Id
                               : part.Id;
            return Combined(
                ContentShape("Parts_Localization_ContentTranslations",
                             () => shapeHelper.Parts_Localization_ContentTranslations(ContentPart: part, MasterId: masterId, Localizations: GetDisplayLocalizations(part, VersionOptions.Published))),
                ContentShape("Parts_Localization_ContentTranslations_Summary",
                             () => shapeHelper.Parts_Localization_ContentTranslations_Summary(ContentPart: part, MasterId: masterId, Localizations: GetDisplayLocalizations(part, VersionOptions.Published))),
                ContentShape("Parts_Localization_ContentTranslations_SummaryAdmin",
                             () => shapeHelper.Parts_Localization_ContentTranslations_SummaryAdmin(ContentPart: part, MasterId: masterId, Localizations: GetDisplayLocalizations(part, VersionOptions.Latest)))
                );
        }

        protected override DriverResult Editor(LocalizationPart part, dynamic shapeHelper) {
            var localizations = GetEditorLocalizations(part).ToList();
            var model = new EditLocalizationViewModel {
                SelectedCulture = part.Culture != null ? part.Culture.Culture : null,
                SiteCultures = _cultureManager.ListCultures().Where(s => s != _cultureManager.GetSiteCulture() && !localizations.Select(l => l.Culture.Culture).Contains(s)),
                ContentItem = part,
                MasterContentItem = part.MasterContentItem,
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
    }
}