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
            // When saving a Taxonomy translation I automatically move to the taxonomy any term
            // in the corresponding language associated to the other translations
            bool termsMoved = false;
            // of course we require the current taxonomy be localized
            if (context.ContentItem.Has<LocalizationPart>()) {
                var taxonomyCulture = context.ContentItem.As<LocalizationPart>().Culture;
                if (taxonomyCulture != null) {
                    // get all localizations of the current taxonomy (except the current taxonomy itself)
                    var taxonomyLocalizations = _localizationService
                        .GetLocalizations(context.ContentItem)
                        .Where(lp => lp.Id != context.ContentItem.Id);
                    foreach (var localizationPart in taxonomyLocalizations) {
                        // in each localization, we should look for the terms that are already
                        // in the culture of the current taxonomy to move them here.
                        var parentCulture = localizationPart.Culture;
                        var terms = _taxonomyService
                            // all the terms in the localized taxonomy for this iteration
                            .GetTerms(localizationPart.Id)
                            .Where(t => {
                                // term should have a LocalizationPart for the analysis here
                                var lp = t.As<LocalizationPart>();
                                if (lp == null) {
                                    return false;
                                }
                                var tc = lp.Culture;
                                // the culture of the term should be the same as our current taxonomy,
                                // and different from its current taxonomy
                                return tc != null && tc != parentCulture && tc == taxonomyCulture;
                            });
                        termsMoved = terms.Any();
                        foreach (var term in terms) {
                            // moving a term moves its children as well. This means that we may
                            // have already moved a term if we moved its parent.
                            if (term.TaxonomyId != part.Id) {
                                // we use the service method to make the move, because that
                                // will take care of recomputing all weights
                                _taxonomyService.MoveTerm(part, term, null);
                            }
                            // update the autoroute so it respects the rules
                            _taxonomyExtensionsService.RegenerateAutoroute(term.ContentItem);
                        }
                    }
                }
            }

            if (termsMoved)
                _notifier.Add(NotifyType.Information, T("Terms in the chosen language have been automatically moved from the other translations."));
        }
    }
}