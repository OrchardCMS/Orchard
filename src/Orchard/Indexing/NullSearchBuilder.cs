using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Indexing {
    public class NullSearchBuilder : ISearchBuilder {
        public ISearchBuilder Parse(string defaultField, string query, bool escape) {
            return this; 
        }

        public ISearchBuilder Parse(string[] defaultFields, string query, bool escape) {
            return this;
        }

        public ISearchBuilder WithGroup(Action<ISearchBuilder> groupSearchBuilder) {
            return this;
        }

        public ISearchBuilder WithField(string field, bool value) {
            return this;
        }

        public ISearchBuilder WithField(string field, DateTime value) {
            return this;
        }

        public ISearchBuilder WithField(string field, string value) {
            return this;
        }

        public ISearchBuilder WithField(string field, int value) {
            return this;
        }

        public ISearchBuilder WithField(string field, double value) {
            return this;
        }

        public ISearchBuilder WithinRange(string field, int? min, int? max, bool includeMin = true, bool includeMax = true) {
            return this;
        }

        public ISearchBuilder WithinRange(string field, double? min, double? max, bool includeMin = true, bool includeMax = true) {
            return this;
        }

        public ISearchBuilder WithinRange(string field, DateTime? min, DateTime? max, bool includeMin = true, bool includeMax = true) {
            return this;
        }

        public ISearchBuilder WithinRange(string field, string min, string max, bool includeMin = true, bool includeMax = true) {
            return this;
        }

        /// <summary>
        /// Mark a clause as a mandatory match. By default all clauses are optional.
        /// </summary>
        public ISearchBuilder Mandatory() {
            return this;
        }

        /// <summary>
        /// Mark a clause as a forbidden match.
        /// </summary>
        public ISearchBuilder Forbidden() {
            return this;
        }

        /// <summary>
        /// Applied on string clauses, it removes the default Prefix mecanism. Like 'broadcast' won't
        /// return 'broadcasting'. 
        /// </summary>
        public ISearchBuilder ExactMatch() {
            return this;
        }

        public ISearchBuilder NotAnalyzed() {
            return this;
        }

        /// <summary>
        /// Apply a specific boost to a clause.
        /// </summary>
        /// <param name="weight">A value greater than zero, by which the score will be multiplied. 
        /// If greater than 1, it will improve the weight of a clause</param>
        public ISearchBuilder Weighted(float weight) {
            return this;
        }

        /// <summary>
        /// Defines a clause as a filter, so that it only affect the results of the other clauses.
        /// For instance, if the other clauses returns nothing, even if this filter has matches the
        /// end result will be empty. It's like a two-pass query
        /// </summary>
        public ISearchBuilder AsFilter() {
            return this;
        }

        public ISearchBuilder SortBy(string name) {
            return this;
        }

        public ISearchBuilder SortByInteger(string name) {
            return this;
        }

        public ISearchBuilder SortByBoolean(string name) {
            return this;
        }

        public ISearchBuilder SortByString(string name) {
            return this;
        }

        public ISearchBuilder SortByDouble(string name) {
            return this;
        }

        public ISearchBuilder SortByDateTime(string name) {
            return this;
        }

        public ISearchBuilder Ascending()
        {
            return this;
        }

        public ISearchBuilder Slice(int skip, int count) {
            return this;
        }

        public IEnumerable<ISearchHit> Search() {
            return Enumerable.Empty<ISearchHit>();
        }
        public ISearchHit Get(int documentId) {
            return null;
        }

        public ISearchBits GetBits() {
            throw new NotImplementedException();
        }

        public int Count() {
            return 0;
        }

    }
}
