using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lucene.Models;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Orchard.Indexing;
using Orchard.Logging;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;

namespace Lucene.Services {
    public class LuceneSearchBuilder : ISearchBuilder {

        private const int MaxResults = Int16.MaxValue;

        private readonly Directory _directory;

        private readonly List<BooleanClause> _clauses;
        private readonly List<BooleanClause> _filters;
        private int _count;
        private int _skip;
        private string _sort;
        private bool _sortDescending;
        private bool _asFilter;

        // pending clause attributes
        private BooleanClause.Occur _occur;
        private bool _exactMatch;
        private float _boost;
        private Query _query;

        public ILogger Logger { get; set; }

        public LuceneSearchBuilder(Directory directory) {
            _directory = directory;
            Logger = NullLogger.Instance;

            _count = MaxResults;
            _skip = 0;
            _clauses = new List<BooleanClause>();
            _filters = new List<BooleanClause>();
            _sort = String.Empty;
            _sortDescending = true;

            InitPendingClause();
        }
        public ISearchBuilder Parse(string defaultField, string query) {
            return Parse(new string[] {defaultField}, query);
        }
        
        public ISearchBuilder Parse(string[] defaultFields, string query) {
            if ( defaultFields.Length == 0 ) {
                throw new ArgumentException("Default field can't be empty");
            }

            if ( String.IsNullOrWhiteSpace(query) ) {
                throw new ArgumentException("Query can't be empty");
            }

            var analyzer = LuceneIndexProvider.CreateAnalyzer();
            foreach ( var defaultField in defaultFields ) {
                var clause = new BooleanClause(new QueryParser(LuceneIndexProvider.LuceneVersion, defaultField, analyzer).Parse(query), BooleanClause.Occur.SHOULD);
                _clauses.Add(clause);
            }
            
            _query = null;
            return this;
        }

        public ISearchBuilder WithField(string field, int value) {
            CreatePendingClause();
            _query = NumericRangeQuery.NewIntRange(field, value, value, true, true);
            return this;
        }

        public ISearchBuilder WithinRange(string field, int min, int max) {
            CreatePendingClause();
            _query = NumericRangeQuery.NewIntRange(field, min, max, true, true);
            return this;
        }

        public ISearchBuilder WithField(string field, float value) {
            CreatePendingClause();
            _query = NumericRangeQuery.NewFloatRange(field, value, value, true, true);
            return this;
        }

        public ISearchBuilder WithinRange(string field, float min, float max) {
            CreatePendingClause();
            _query = NumericRangeQuery.NewFloatRange(field, min, max, true, true);
            return this;
        }

        public ISearchBuilder WithField(string field, bool value) {
            CreatePendingClause();
            _query = new TermQuery(new Term(field, value.ToString()));
            return this;
        }

        public ISearchBuilder WithField(string field, DateTime value) {
            CreatePendingClause();
            _query = new TermQuery(new Term(field, DateTools.DateToString(value, DateTools.Resolution.SECOND)));
            return this;
        }

        public ISearchBuilder WithinRange(string field, DateTime min, DateTime max) {
            CreatePendingClause();
            _query = new TermRangeQuery(field, DateTools.DateToString(min, DateTools.Resolution.SECOND), DateTools.DateToString(max, DateTools.Resolution.SECOND), true, true);
            return this;
        }

        public ISearchBuilder WithinRange(string field, string min, string max) {
            CreatePendingClause();
            _query = new TermRangeQuery(field, QueryParser.Escape(min.ToLower()), QueryParser.Escape(min.ToLower()), true, true);
            return this;
        }

        public ISearchBuilder WithField(string field, string value) {
            CreatePendingClause();

            if ( !String.IsNullOrWhiteSpace(value) ) {
                _query = new TermQuery(new Term(field, QueryParser.Escape(value.ToLower())));
            }
            
            return this;
        }

        public ISearchBuilder Mandatory() {
            _occur = BooleanClause.Occur.MUST;
            return this;
        }

        public ISearchBuilder Forbidden() {
            _occur = BooleanClause.Occur.MUST_NOT;
            return this;
        }

        public ISearchBuilder ExactMatch() {
            _exactMatch = true;
            return this;
        }

        public ISearchBuilder Weighted(float weight) {
            _boost = weight;
            return this;
        }

        private void InitPendingClause() {
            _occur = BooleanClause.Occur.SHOULD;
            _exactMatch = false;
            _query = null;
            _boost = 0;
            _asFilter = false;
        }

        private void CreatePendingClause() {
            if(_query == null) {
                return;
            }

            if (_boost != 0) {
                _query.SetBoost(_boost);
            }

            if(!_exactMatch) {
                var termQuery = _query as TermQuery;
                if(termQuery != null) {
                    _query = new PrefixQuery(termQuery.GetTerm());
                }
            }
            if ( _asFilter ) {
                _filters.Add(new BooleanClause(_query, _occur));
            }
            else {
                _clauses.Add(new BooleanClause(_query, _occur));
            }
        }

        public ISearchBuilder SortBy(string name) {
            _sort = name;
            return this;
        }

        public ISearchBuilder Ascending() {
            _sortDescending = false;
            return this;
        }

        public ISearchBuilder AsFilter() {
            _asFilter = true;
            return this;
        }

        public ISearchBuilder Slice(int skip, int count) {
            if ( skip < 0 ) {
                throw new ArgumentException("Skip must be greater or equal to zero");
            }

            if ( count <= 0 ) {
                throw new ArgumentException("Count must be greater than zero");
            }

            _skip = skip;
            _count = count;
            
            return this;
        }

        private Query CreateQuery() {
            CreatePendingClause();

            var query = new BooleanQuery();

            foreach( var clause in _clauses)
                query.Add(clause);
           

            if ( query.Clauses().Count == 0 ) { // get all documents ?
                query.Add(new TermRangeQuery("id", "0", "9", true, true), BooleanClause.Occur.SHOULD);
            }

            Query finalQuery = query;

            if(_filters.Count > 0) {
                var filter = new BooleanQuery();
                foreach( var clause in _filters)
                    filter.Add(clause);
                var queryFilter = new QueryWrapperFilter(filter);

                finalQuery = new FilteredQuery(query, queryFilter);
            }

            Logger.Debug("New search query: {0}", finalQuery.ToString());
            return finalQuery;
        }

        public IEnumerable<ISearchHit> Search() {
            var query = CreateQuery();
            
            IndexSearcher searcher;

            try {
                searcher = new IndexSearcher(_directory, true);
            }
            catch {
                // index might not exist if it has been rebuilt
                Logger.Information("Attempt to read a none existing index");
                return Enumerable.Empty<ISearchHit>();
            }

            try {
                var sort = String.IsNullOrEmpty(_sort)
                               ? Sort.RELEVANCE
                               : new Sort(new SortField(_sort, CultureInfo.InvariantCulture, _sortDescending));
                var collector = TopFieldCollector.create(
                    sort,
                    _count + _skip,
                    false,
                    true,
                    false,
                    true);

                Logger.Debug("Searching: {0}", query.ToString());
                searcher.Search(query, collector);

                var results = collector.TopDocs().scoreDocs
                    .Skip(_skip)
                    .Select(scoreDoc => new LuceneSearchHit(searcher.Doc(scoreDoc.doc), scoreDoc.score))
                    .ToList();

                Logger.Debug("Search results: {0}", results.Count);

                return results;
            }
            finally {
                searcher.Close();
            }

        }

        public int Count() {
            var query = CreateQuery();
            IndexSearcher searcher;
            
            try {
                 searcher = new IndexSearcher(_directory, true);
            }
            catch {
                // index might not exist if it has been rebuilt
                Logger.Information("Attempt to read a none existing index");
                return 0;
            }

            try {
                var hits = searcher.Search(query, Int16.MaxValue);
                Logger.Information("Search results: {0}", hits.scoreDocs.Length);
                var length = hits.scoreDocs.Length;
                return Math.Min(length - _skip, _count) ;
            }
            finally {
                searcher.Close();
            }
            
        }

        public ISearchHit Get(int documentId) {
            var query = new TermQuery(new Term("id", documentId.ToString()));

            var searcher = new IndexSearcher(_directory, true);
            try {
                var hits = searcher.Search(query, 1);
                Logger.Information("Search results: {0}", hits.scoreDocs.Length);
                if ( hits.scoreDocs.Length > 0 ) {
                    return new LuceneSearchHit(searcher.Doc(hits.scoreDocs[0].doc), hits.scoreDocs[0].score);
                }
                else {
                    return null;
                }
            }
            finally {
                searcher.Close();
            }
        }

    }
}
