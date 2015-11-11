using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public class TermCountProcessor : ITermCountProcessor {
        private readonly ITaxonomyService _taxonomyService;

        public TermCountProcessor(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        public void Process(IEnumerable<int> termPartRecordIds)
        {
            var processedTermPartRecordIds = new List<int>();
            foreach (var id in termPartRecordIds) {
                if (!processedTermPartRecordIds.Contains(id)) {
                    var termPart = _taxonomyService.GetTerm(id);
                    if (termPart != null) {
                        ProcessTerm(termPart, processedTermPartRecordIds);
                    }
                }
            }
        }

        private void ProcessTerm(TermPart termPart, ICollection<int> processedTermPartRecordIds)
        {
            termPart.Count = (int)_taxonomyService.GetContentItemsCount(termPart);
            processedTermPartRecordIds.Add(termPart.Id);

            // Look for a parent term that has not yet been processed
            if (termPart.Container != null) {
                var parentTerm = termPart.Container.As<TermPart>();
                if (parentTerm != null && !processedTermPartRecordIds.Contains(parentTerm.Id)) {
                    ProcessTerm(parentTerm, processedTermPartRecordIds);
                }
            }
        }
    }
}
