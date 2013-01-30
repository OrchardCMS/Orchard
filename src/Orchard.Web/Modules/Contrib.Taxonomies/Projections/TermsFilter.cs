using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;

namespace Contrib.Taxonomies.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class TermsFilter : IFilterProvider {
        private readonly ITaxonomyService _taxonomyService;

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
            string termIds = Convert.ToString(context.State.TermIds);
            if (!String.IsNullOrEmpty(termIds)) {
                var ids = termIds.Split(new[] { ',' }).Select(Int32.Parse).ToArray();

                if (ids.Length == 0) {
                    return;
                }

                var terms = ids.Select(_taxonomyService.GetTerm).ToList();

                var predicates = new List<Action<IHqlExpressionFactory>>();

                foreach(var term in terms) {
                    var localTerm = term;
                    Action<IHqlExpressionFactory> ors = x => x.Or(a => a.Eq("Id", ids.First()), b => b.Like("Path", localTerm.FullPath + "/", HqlMatchMode.Start));
                    predicates.Add(ors);
                }
                
                Action<IAliasFactory> selector = alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms").Property("TermRecord", "termRecord");
                Action<IHqlExpressionFactory> filter = x => x.Conjunction(predicates.Take(1).Single(), predicates.Skip(1).ToArray());
                context.Query.Where(selector, filter);
            }
        }

        public LocalizedString DisplayFilter(dynamic context) {
            string terms = Convert.ToString(context.State.TermIds);

            if (String.IsNullOrEmpty(terms)) {
                return T("Any term");
            }

            var tagNames = terms.Split(new[] { ',' }).Select(x => _taxonomyService.GetTerm(Int32.Parse(x)).Name);

            return T("Categorized with {0}", String.Join(", ", tagNames));
        }
    }

}