using System;
using System.Collections.Generic;

namespace Orchard.Indexing {
    public interface ISearchBuilder {

        ISearchBuilder Parse(string query);

        ISearchBuilder WithField(string field, string value);
        ISearchBuilder WithField(string field, string value, bool wildcardSearch);

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
