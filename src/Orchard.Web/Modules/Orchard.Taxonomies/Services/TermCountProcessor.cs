using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public class TermCountProcessor : ITermCountProcessor {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public TermCountProcessor(IContentManager contentManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }
        
        public void Process(int termsPartId) {
            var termsPart = _contentManager.Get<TermsPart>(termsPartId);

            if (termsPart == null) {
                return;
            }

            // Retrieve the number of associated content items, for the whole hierarchy
            foreach (var term in termsPart.Terms) {
                var termPart = _taxonomyService.GetTerm(term.TermRecord.Id);
                while (termPart != null) {
                    termPart.Count = (int)_taxonomyService.GetContentItemsCount(termPart);

                    // compute count for the hierarchy too
                    if (termPart.Container != null) {
                        var parentTerm = termPart.Container.As<TermPart>();
                        termPart = parentTerm;
                    }
                }
            }
        }
    }
}