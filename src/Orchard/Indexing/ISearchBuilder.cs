using System;
using System.Collections.Generic;

namespace Orchard.Indexing {
    public interface ISearchBuilder {
        ISearchBuilder Parse(string defaultField, string query);
        ISearchBuilder Parse(string[] defaultFields, string query);

        ISearchBuilder WithField(string field, bool value);
        ISearchBuilder WithField(string field, DateTime value);
        ISearchBuilder WithField(string field, string value);
        ISearchBuilder WithField(string field, int value);
        ISearchBuilder WithField(string field, float value);
        ISearchBuilder WithinRange(string field, int min, int max);
        ISearchBuilder WithinRange(string field, float min, float max);
        ISearchBuilder WithinRange(string field, DateTime min, DateTime max);
        ISearchBuilder WithinRange(string field, string min, string max);

        ISearchBuilder Mandatory();
        ISearchBuilder Forbidden();
        ISearchBuilder ExactMatch();
        ISearchBuilder Weighted(float weight);

        ISearchBuilder SortBy(string name);
        ISearchBuilder Ascending();

        ISearchBuilder Slice(int skip, int count);
        IEnumerable<ISearchHit> Search();
        ISearchHit Get(int documentId);
        int Count();


    }
}
