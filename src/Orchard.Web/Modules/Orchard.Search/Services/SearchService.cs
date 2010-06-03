using System.Collections.Generic;
using System.Linq;
using Orchard.Indexing;

namespace Orchard.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly IIndexProvider _indexProvider;

        public SearchService(IIndexProvider indexProvider) {
            _indexProvider = indexProvider;
        }

        public IEnumerable<ISearchHit> Query(string term) {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<ISearchHit>();

            return _indexProvider.CreateSearchBuilder("search")
                .WithField("title", term)
                .WithField("body", term)
                .Search();
        }
    }
}