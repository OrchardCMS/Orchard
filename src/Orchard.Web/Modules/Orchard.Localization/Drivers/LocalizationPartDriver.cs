using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;

namespace Orchard.Localization.Drivers {
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

            return Combined(
                ContentShape("Parts_Localization_ContentTranslations",
                             () => shapeHelper.Parts_Localization_ContentTranslations(Id: part.ContentItem.Id, MasterId: ActualMasterId(part), Culture: GetCulture(part), Localizations: GetDisplayLocalizations(part, VersionOptions.Published))),
                ContentShape("Parts_Localization_ContentTranslations_Summary",
                             () => shapeHelper.Parts_Localization_ContentTranslations_Summary(Id: part.ContentItem.Id, MasterId: ActualMasterId(part), Culture: GetCulture(part), Localizations: GetDisplayLocalizations(part, VersionOptions.Published))),
                ContentShape("Parts_Localization_ContentTranslations_SummaryAdmin", () => {
                    var siteCultures = _cultureManager.ListCultures();

                    return shapeHelper.Parts_Localization_ContentTranslations_SummaryAdmin(Id: part.ContentItem.Id, MasterId: ActualMasterId(part), Culture: GetCulture(part), Localizations: GetDisplayLocalizations(part, VersionOptions.Latest), SiteCultures: siteCultures);
                })
                );
        }

        private int ActualMasterId(LocalizationPart part) {
            var masterId = part.HasTranslationGroup
                               ? part.Record.MasterContentItemId
                               : part.Id;
            if (_contentManager.Get(masterId, VersionOptions.Latest) == null) {
                //the original MasterContentItem has been deleted
                masterId = part.Id;
            }
            return masterId;
        }

        protected override DriverResult Editor(LocalizationPart part, dynamic shapeHelper) {
            var localizations = GetEditorLocalizations(part).ToList();

            var masterContentItem = _contentManager.Get(part.Record.MasterContentItemId, VersionOptions.Latest);

            var missingCultures = part.HasTranslationGroup && masterContentItem != null ?
                RetrieveMissingCultures(masterContentItem.As<LocalizationPart>(), true) :
                RetrieveMissingCultures(part, part.Culture != null);

            var model = new EditLocalizationViewModel {
                SelectedCulture = GetCulture(part),
                MissingCultures = missingCultures,
                ContentItem = part,
                MasterContentItem = masterContentItem,
                ContentLocalizations = new ContentLocalizationsViewModel(part) { Localizations = localizations }
            };

            return ContentShape("Parts_Localization_ContentTranslations_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Localization.ContentTranslations.Edit", Model: model, Prefix: TemplatePrefix));
        }

        protected override DriverResult Editor(LocalizationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new EditLocalizationViewModel();

            // GetCulture(part) is checked against null value, because the content culture has to be set only if it's not set already.
            // model.SelectedCulture is checked against null value, because the editor group may not contain LocalizationPart when the content item is saved for the first time.
            if (updater != null && updater.TryUpdateModel(model, TemplatePrefix, null, null) && GetCulture(part) == null && !string.IsNullOrEmpty(model.SelectedCulture)) {
                _localizationService.SetContentCulture(part, model.SelectedCulture);
            }

            return Editor(part, shapeHelper);
        }

        private List<string> RetrieveMissingCultures(LocalizationPart part, bool excludePartCulture) {
            var editorLocalizations = GetEditorLocalizations(part.MasterContentItem != null ? part.MasterContentItem.As<LocalizationPart>() : part);

            var cultures = _cultureManager
                .ListCultures()
                .Where(s => editorLocalizations.All(l => l.Culture.Culture != s))
                .ToList();

            if (excludePartCulture) {
                cultures.Remove(part.Culture.Culture);
            }

            return cultures;
        }

        private static string GetCulture(LocalizationPart part) {
            return part.Culture != null ? part.Culture.Culture : null;
        }

        private IEnumerable<LocalizationPart> GetDisplayLocalizations(LocalizationPart part, VersionOptions versionOptions) {
            return _localizationService.GetLocalizations(part.ContentItem, versionOptions)
                .Where(c => c.Culture != null)
                .ToList();
        }

        private IEnumerable<LocalizationPart> GetEditorLocalizations(LocalizationPart part) {
            return _localizationService.GetLocalizations(part.ContentItem, VersionOptions.Latest)
                .Where(c => c.Culture != null)
                .ToList();
        }

        protected override void Importing(LocalizationPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "MasterContentItem", masterContentItem => {
                var contentItem = context.GetItemFromSession(masterContentItem);
                if (contentItem != null) {
                    part.MasterContentItem = contentItem;
                }
            });

            context.ImportAttribute(part.PartDefinition.Name, "Culture", culture => {
                var targetCulture = _cultureManager.GetCultureByName(culture);
                // Add Culture.
                if (targetCulture == null && _cultureManager.IsValidCulture(culture)) {
                    _cultureManager.AddCulture(culture);
                    targetCulture = _cultureManager.GetCultureByName(culture);
                }
                part.Culture = targetCulture;
            });
        }

        protected override void Exporting(LocalizationPart part, ExportContentContext context) {
            if (part.MasterContentItem != null) {
                var masterContentItemIdentity = _contentManager.GetItemMetadata(part.MasterContentItem).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("MasterContentItem", masterContentItemIdentity.ToString());
            }

            if (part.Culture != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("Culture", part.Culture.Culture);
            }
        }

        protected override void Cloned(LocalizationPart originalPart, LocalizationPart clonePart, CloneContentContext context) {
            clonePart.Culture = originalPart.Culture;
        }
    }
}