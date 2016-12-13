using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.Handlers {
    public class LocalizationTaxonomyFieldHandler : ContentHandler {

        private readonly INotifier _notifier;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        private readonly IContentManager _contentManager;

        public Localizer T { get; set; }

        public LocalizationTaxonomyFieldHandler(
                ILocalizationService localizationService,
                INotifier notifier,
                ITaxonomyService taxonomyService,
                    ITaxonomyExtensionsService taxonomyExtensionsService,
                IContentManager contentManager
            ) {
            _notifier = notifier;
            _localizationService = localizationService;
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            base.UpdateEditorShape(context);
            var contentItemCulture = _localizationService.GetContentCulture(context.ContentItem);
            var taxonomyIds = _taxonomyService.GetTermsForContentItem(context.ContentItem.Id).Select(x => x.TaxonomyId).Distinct<int>();
            foreach (var taxonomyId in taxonomyIds) {
                var taxonomyofTerm = _contentManager.Get(taxonomyId);
                var mastertaxonomy = _taxonomyExtensionsService.GetMasterItem(taxonomyofTerm);
                var localizedTaxonomies = _localizationService.GetLocalizations(mastertaxonomy);
                List<string> TitlesTaxonomy = new List<string>();
                TitlesTaxonomy.Add(mastertaxonomy.As<TitlePart>().Title);
                foreach (var tax in localizedTaxonomies) {
                    TitlesTaxonomy.Add(tax.As<TitlePart>().Title);
                }
                if (contentItemCulture != _localizationService.GetContentCulture(taxonomyofTerm)) {
                    var taxonomyField = context.ContentItem.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField" && TitlesTaxonomy.Contains(x.Settings["TaxonomyFieldSettings.Taxonomy"])).FirstOrDefault();
                    var taxonomyLocalizationSettings = taxonomyField.Settings.GetModel<TaxonomyFieldLocalizationSettings>();
                    if (taxonomyLocalizationSettings.TryToLocalize) {
                        var termsParts = _taxonomyService.GetTermsForContentItem(context.ContentItem.Id).Where(x => x.TaxonomyId == taxonomyId);
                        List<TermPart> checkedTerms = new List<TermPart>();
                        foreach (var termPart in termsParts) {
                            if (termPart.ContentItem.As<LocalizationPart>() != null) {
                                var termPartLocalized = _localizationService.GetLocalizedContentItem(_taxonomyExtensionsService.GetMasterItem(termPart.ContentItem), contentItemCulture).As<TermPart>();
                                if (termPartLocalized != null) {
                                    checkedTerms.Add(termPartLocalized);
                                    _notifier.Add(NotifyType.Warning, T("Term {0} has been localized to {1}", termPart.ContentItem.As<TitlePart>().Title, termPartLocalized.ContentItem.As<TitlePart>().Title));
                                }
                                else {
                                    if (taxonomyLocalizationSettings.RemoveItemsWithoutLocalization)
                                        _notifier.Add(NotifyType.Warning, T("Term {0} has been removed because localized version is missing", termPart.ContentItem.As<TitlePart>().Title));
                                    else
                                        context.Updater.AddModelError("Localization error", T("Term {0} isn't in the same culture of content", termPart.ContentItem.As<TitlePart>().Title));
                                }
                            }
                            else
                                checkedTerms.Add(termPart);
                        }
                        _taxonomyService.UpdateTerms(context.ContentItem, checkedTerms, taxonomyField.Name);
                    }
                    else
                        context.Updater.AddModelError("Localization error", T("The taxonomy {0} isn't in the same culture of content", taxonomyofTerm.As<TitlePart>().Title));
                }
            }
        }
    }
}