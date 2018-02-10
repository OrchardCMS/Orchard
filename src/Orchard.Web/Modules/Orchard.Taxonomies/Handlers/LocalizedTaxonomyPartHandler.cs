using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.Handlers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyPartHandler : ContentHandler {
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;

        public LocalizedTaxonomyPartHandler(
            ILocalizationService localizationService,
            INotifier notifier,
            ITaxonomyService taxonomyService,
            ITaxonomyExtensionsService taxonomyExtensionsService) {
            _localizationService = localizationService;
            _notifier = notifier;
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;

            T = NullLocalizer.Instance;

            OnPublishing<TaxonomyPart>((context, part) => ImportLocalizedTerms(context, part));
        }

        public Localizer T { get; set; }

        private void ImportLocalizedTerms(PublishContentContext context, TaxonomyPart part) {
            // When saving a Taxonomy translation I automatically move to the taxonomy any term in the corresponding language associated to the other translations
            bool termsMoved = false;

            if (context.ContentItem.Has<LocalizationPart>()) {
                var taxonomyCulture = context.ContentItem.As<LocalizationPart>().Culture;
                if (taxonomyCulture != null) {
                    var taxonomyIds = _localizationService.GetLocalizations(context.ContentItem).Select(x => x.Id);

                    foreach (var taxonomyId in taxonomyIds) {
                        var parentTaxonomyCulture = _taxonomyService.GetTaxonomy(taxonomyId).As<LocalizationPart>().Culture;
                        var termIds = _taxonomyService.GetTerms(taxonomyId).Select(x => x.Id);

                        foreach (var termId in termIds) {
                            var term = _taxonomyService.GetTerm(termId);
                            var parentTerm = _taxonomyExtensionsService.GetParentTerm(term);
                            if (term.Has<LocalizationPart>()) {
                                var termCulture = term.As<LocalizationPart>().Culture;

                                if (termCulture != null && termCulture != parentTaxonomyCulture && termCulture == taxonomyCulture) {
                                    term.TaxonomyId = context.ContentItem.Id;
                                    term.Path = parentTerm != null ? parentTerm.As<TermPart>().FullPath + "/" : "/";
                                    if (parentTerm == null)
                                        term.Container = context.ContentItem;

                                    _taxonomyExtensionsService.RegenerateAutoroute(term.ContentItem);

                                    termsMoved = true;
                                }
                            }
                        }
                    }
                }
            }

            if (termsMoved)
                _notifier.Add(NotifyType.Information, T("Terms in the chosen language have been automatically moved from the other translations."));
        }
    }
}