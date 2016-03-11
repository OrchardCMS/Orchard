using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Taxonomies.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class TermsFilter : IFilterProvider {
        private readonly ITaxonomyService _taxonomyService;
        private int _termsFilterId;

        public TermsFilter(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Taxonomy", T("Taxonomy"), T("Taxonomy"))
                .Element("HasTerms", T("Has Terms"), T("Categorized content items"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "SelectTerms"
                );
        }

        public void ApplyFilter(dynamic context) {
            var termIds = (string)context.State.TermIds;

            if (!String.IsNullOrEmpty(termIds)) {
                var ids = termIds.Split(new[] { ',' }).Select(Int32.Parse).ToArray();

                if (ids.Length == 0) {
                    return;
                }

                int op = Convert.ToInt32(context.State.Operator);

                var terms = ids.Select(_taxonomyService.GetTerm).ToList();
                var allChildren = new List<TermPart>();
                foreach (var term in terms) {
                    allChildren.AddRange(_taxonomyService.GetChildren(term));
                    allChildren.Add(term);
                }

                allChildren = allChildren.Distinct().ToList();

                var allIds = allChildren.Select(x => x.Id).ToList();

                switch (op) {
                    case 0: // is one of
                        // Unique alias so we always get a unique join everytime so can have > 1 HasTerms filter on a query.
                        Action<IAliasFactory> s = alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms" + _termsFilterId++);
                        Action<IHqlExpressionFactory> f = x => x.InG("TermRecord.Id", allIds);
                        context.Query.Where(s, f);
                        break;
                    case 1: // is all of
                        foreach (var id in allIds) {
                            var termId = id;
                            Action<IAliasFactory> selector =
                                alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms" + termId);
                            Action<IHqlExpressionFactory> filter = x => x.Eq("TermRecord.Id", termId);
                            context.Query.Where(selector, filter);
                        }
                        break;
                }
            }
        }

        public LocalizedString DisplayFilter(dynamic context) {
            var terms = (string)context.State.TermIds;

            if (String.IsNullOrEmpty(terms)) {
                return T("Any term");
            }

            var tagNames = terms.Split(new[] { ',' }).Select(x => _taxonomyService.GetTerm(Int32.Parse(x)).Name);

            int op = Convert.ToInt32(context.State.Operator);
            switch (op) {
                case 0:
                    return T("Categorized with one of {0}", String.Join(", ", tagNames));

                case 1:
                    return T("Categorized with all of {0}", String.Join(", ", tagNames));
            }

            return T("Categorized with {0}", String.Join(", ", tagNames));
        }
    }
}