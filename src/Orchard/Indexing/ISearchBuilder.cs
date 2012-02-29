using System;
using System.Collections.Generic;

namespace Orchard.Indexing {
    public interface ISearchBuilder {
        ISearchBuilder Parse(string defaultField, string query, bool escape = true);
        ISearchBuilder Parse(string[] defaultFields, string query, bool escape = true);

        ISearchBuilder WithField(string field, bool value);
        ISearchBuilder WithField(string field, DateTime value);
        ISearchBuilder WithField(string field, string value);
        ISearchBuilder WithField(string field, int value);
        ISearchBuilder WithField(string field, double value);
        ISearchBuilder WithinRange(string field, int min, int max);
        ISearchBuilder WithinRange(string field, double min, double max);
        ISearchBuilder WithinRange(string field, DateTime min, DateTime max);
        ISearchBuilder WithinRange(string field, string min, string max);

        /// <summary>
        /// Mark a clause as a mandatory match. By default all clauses are optional.
        /// </summary>
        ISearchBuilder Mandatory();

        /// <summary>
        /// Mark a clause as a forbidden match.
        /// </summary>
        ISearchBuilder Forbidden();

        /// <summary>
        /// Applied on string clauses, it removes the default Prefix mecanism. Like 'broadcast' won't
        /// return 'broadcasting'. 
        /// </summary>
        ISearchBuilder ExactMatch();
        
        /// <summary>
        /// Apply a specific boost to a clause.
        /// </summary>
        /// <param name="weight">A value greater than zero, by which the score will be multiplied. 
        /// If greater than 1, it will improve the weight of a clause</param>
        ISearchBuilder Weighted(float weight);

        /// <summary>
        /// Defines a clause as a filter, so that it only affect the results of the other clauses.
        /// For instance, if the other clauses returns nothing, even if this filter has matches the
        /// end result will be empty. It's like a two-pass query
        /// </summary>
        ISearchBuilder AsFilter();

        ISearchBuilder SortBy(string name);
        ISearchBuilder SortByInteger(string name);
        ISearchBuilder SortByBoolean(string name);
        ISearchBuilder SortByString(string name);
        ISearchBuilder SortByDouble(string name);
        ISearchBuilder SortByDateTime(string name);
        ISearchBuilder Ascending();

        ISearchBuilder Slice(int skip, int count);
        IEnumerable<ISearchHit> Search();
        ISearchHit Get(int documentId);
        int Count();

    }
}
