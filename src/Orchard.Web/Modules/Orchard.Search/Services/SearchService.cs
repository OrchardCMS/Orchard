using System.Collections.Generic;
using System.Linq;
using Orchard.Indexing;

namespace Orchard.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly IIndexManager _indexManager;

        public SearchService(IIndexManager indexManager) {
            _indexManager = indexManager;
        }

        public IEnumerable<ISearchHit> Query(string term) {
            if (string.IsNullOrWhiteSpace(term) || !_indexManager.HasIndexProvider())
                return Enumerable.Empty<ISearchHit>();

            return _indexManager.GetSearchIndexProvider().CreateSearchBuilder("search")
                .WithField("title", term)
                .WithField("body", term)
                .Search();
        }
    }
}