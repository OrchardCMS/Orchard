using System;
using System.Collections.Generic;

namespace Orchard.Indexing {
    public interface ISearchBuilder {
        ISearchBuilder Parse(string[] defaultFields, string query);

        ISearchBuilder WithField(string field, string value);
        ISearchBuilder Mandatory();
        ISearchBuilder Forbidden();
        ISearchBuilder ExactMatch();
        ISearchBuilder Weighted(float weight);

        ISearchBuilder After(string name, DateTime date);
        ISearchBuilder Before(string name, DateTime date);
        ISearchBuilder SortBy(string name);
        ISearchBuilder Ascending();

        ISearchBuilder Slice(int skip, int count);
        IEnumerable<ISearchHit> Search();
        ISearchHit Get(int documentId);
        int Count();


    }
}
