using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.Handlers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyFieldHandler : ContentHandler {

        private readonly INotifier _notifier;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;

        public Localizer T { get; set; }

        public LocalizedTaxonomyFieldHandler(
                ILocalizationService localizationService,
                INotifier notifier,
                ITaxonomyService taxonomyService,
                ITaxonomyExtensionsService taxonomyExtensionsService,
                IContentManager contentManager,
                ICultureManager cultureManager
            ) {
            _notifier = notifier;
            _localizationService = localizationService;
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        private IEnumerable<LocalizationPart> GetEditorLocalizations(LocalizationPart part) {
            return _localizationService.GetLocalizations(part.ContentItem, VersionOptions.Latest)
                .Where(c => c.Culture != null)
                .ToList();
        }
        private List<string> RetrieveMissingCultures(LocalizationPart part, bool excludePartCulture) {
            var editorLocalizations = GetEditorLocalizations(part);
            var cultures = _cultureManager
                .ListCultures()
                .Where(s => editorLocalizations.All(l => l.Culture.Culture != s))
                .ToList();
            if (excludePartCulture) {
                cultures.Remove(part.Culture.Culture);
            }
            return cultures;
        }

        protected override void BuildEditorShape(BuildEditorContext context) {
            // case new translation of contentitem
            var localizationPart = context.ContentItem.As<LocalizationPart>();
            if (localizationPart == null || localizationPart.Culture != null || context.ContentItem.As<LocalizationPart>().MasterContentItem == null || context.ContentItem.As<LocalizationPart>().MasterContentItem.Id == 0) {
                return;
            }
            var partFieldDefinitions = context.ContentItem.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField");
            if (partFieldDefinitions == null)
                return; // contentitem without taxonomy
            base.BuildEditorShape(context);
            var missingCultures = localizationPart.HasTranslationGroup ?
                RetrieveMissingCultures(localizationPart.MasterContentItem.As<LocalizationPart>(), true) :
                RetrieveMissingCultures(localizationPart, localizationPart.Culture != null);
            foreach (var partFieldDefinition in partFieldDefinitions) {
                if (partFieldDefinition.Settings.GetModel<TaxonomyFieldLocalizationSettings>().TryToLocalize) {
                    var originalTermParts = _taxonomyService.GetTermsForContentItem(context.ContentItem.As<LocalizationPart>().MasterContentItem.Id, partFieldDefinition.Name, VersionOptions.Latest).Distinct(new TermPartComparer()).ToList();
                    var newTermParts = new List<TermPart>();
                    foreach (var originalTermPart in originalTermParts) {
                        var masterTermPart = _taxonomyExtensionsService.GetMasterItem(originalTermPart.ContentItem);
                        if (masterTermPart != null) {
                            foreach (var missingCulture in missingCultures) {
                                var newTerm = _localizationService.GetLocalizedContentItem(masterTermPart, missingCulture);
                                if (newTerm != null)
                                    newTermParts.Add(newTerm.ContentItem.As<TermPart>());
                                else
                                    _notifier.Add(NotifyType.Warning, T("Term {0} can't be localized on {1}, term has been removed on this language", originalTermPart.ContentItem.As<TitlePart>().Title, missingCulture));
                            }
                        }
                        else
                            _notifier.Add(NotifyType.Warning, T("Term {0} can't be localized, term has been removed", originalTermPart.ContentItem.As<TitlePart>().Title));
                    }
                    _taxonomyService.UpdateTerms(context.ContentItem, newTermParts, partFieldDefinition.Name);
                }
            }
        }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            // case contentitem without localization and taxonomyfield localized
            if (context.ContentItem.As<LocalizationPart>() != null) {
                return;
            }
            var partFieldDefinitions = context.ContentItem.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField");
            if (partFieldDefinitions == null || !partFieldDefinitions.Any()) {
                return;
            }
            base.UpdateEditorShape(context);
            var allCultures = _cultureManager.ListCultures();
            if (allCultures.Count() > 1) {
                foreach (var partFieldDefinition in partFieldDefinitions) {
                    if (partFieldDefinition.Settings.GetModel<TaxonomyFieldLocalizationSettings>().TryToLocalize) {
                        var taxonomyUsed = _taxonomyService.GetTaxonomyByName(partFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>().Taxonomy);
                        var originalTermParts = _taxonomyService.GetTermsForContentItem(context.ContentItem.Id, partFieldDefinition.Name, VersionOptions.Latest).Distinct(new TermPartComparer()).ToList();
                        var newTermParts = new List<TermPart>();
                        foreach (var originalTermPart in originalTermParts) {
                            newTermParts.Add(originalTermPart);
                            var masterTermPart = _taxonomyExtensionsService.GetMasterItem(originalTermPart.ContentItem);
                            if (masterTermPart != null) {
                                foreach (var missingCulture in allCultures) {
                                    var newTerm = _localizationService.GetLocalizedContentItem(masterTermPart, missingCulture);
                                    if (newTerm != null)
                                        newTermParts.Add(newTerm.As<TermPart>());
                                    else
                                        _notifier.Add(NotifyType.Warning, T("Term {0} can't be localized on {1}, term has been removed on this language", originalTermPart.ContentItem.As<TitlePart>().Title, missingCulture));
                                }
                            }
                            else
                                _notifier.Add(NotifyType.Warning, T("Term {0} can't be localized", originalTermPart.ContentItem.As<TitlePart>().Title));
                        }
                        _taxonomyService.UpdateTerms(context.ContentItem, newTermParts.Distinct(new TermPartComparer()), partFieldDefinition.Name);
                    }
                }
            }
        }
    }
}
