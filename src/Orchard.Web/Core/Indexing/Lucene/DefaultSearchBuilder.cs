using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Orchard.Logging;
using Lucene.Net.Documents;
using Orchard.Indexing;
using Lucene.Net.QueryParsers;

namespace Orchard.Core.Indexing.Lucene {
    public class DefaultSearchBuilder : ISearchBuilder {

        private const int MaxResults = Int16.MaxValue;

        private readonly Directory _directory;

        private readonly Dictionary<string, Query[]> _fields;
        private int _count;
        private int _skip;
        private readonly Dictionary<string, DateTime> _before;
        private readonly Dictionary<string, DateTime> _after;
        private string _sort;
        private bool _sortDescending;
        private string _parse;
        private readonly Analyzer _analyzer;
        private string _defaultField;

        public ILogger Logger { get; set; }

        public DefaultSearchBuilder(Directory directory) {
            _directory = directory;
            Logger = NullLogger.Instance;

            _count = MaxResults;
            _skip = 0;
            _before = new Dictionary<string, DateTime>();
            _after = new Dictionary<string, DateTime>();
            _fields = new Dictionary<string, Query[]>();
            _sort = String.Empty;
            _sortDescending = true;
            _parse = String.Empty;
            _analyzer = DefaultIndexProvider.CreateAnalyzer();
        }

        public ISearchBuilder Parse(string defaultField, string query) {
            if ( String.IsNullOrWhiteSpace(defaultField) ) {
                throw new ArgumentException("Default field can't be empty");
            }

            if ( String.IsNullOrWhiteSpace(query) ) {
                throw new ArgumentException("Query can't be empty");
            }

            _defaultField = defaultField;
            _parse = query;
            return this;
        }

        public ISearchBuilder WithField(string field, string value) {
            return WithField(field, value, true);
        }

        public ISearchBuilder WithField(string field, string value, bool wildcardSearch) {

            var tokens = new List<string>();
            using(var sr = new System.IO.StringReader(value)) {
                var stream = _analyzer.TokenStream(field, sr);
                while(stream.IncrementToken()) {
                    tokens.Add(((TermAttribute)stream.GetAttribute(typeof(TermAttribute))).Term());
                }
            }

            _fields[field] = tokens
                            .Where(k => !String.IsNullOrWhiteSpace(k))
                            .Select(QueryParser.Escape)
                            .Select(k => wildcardSearch ? (Query)new PrefixQuery(new Term(field, k)) : new TermQuery(new Term(k)))
                            .ToArray();
            
            return this;
        }

        public ISearchBuilder After(string name, DateTime date) {
            _after[name] = date;
            return this;
        }

        public ISearchBuilder Before(string name, DateTime date) {
            _before[name] = date;
            return this;
        }

        public ISearchBuilder SortBy(string name) {
            _sort = name;
            return this;
        }

        public ISearchBuilder Ascending() {
            _sortDescending = false;
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
            if(!String.IsNullOrWhiteSpace(_parse)) {
                return new QueryParser(DefaultIndexProvider.LuceneVersion, _defaultField, DefaultIndexProvider.CreateAnalyzer()).Parse(_parse);    
            }

            var query = new BooleanQuery();

            if ( _fields.Keys.Count > 0 ) {  // apply specific filters if defined
                foreach ( var filters in _fields.Values ) {
                    foreach(var filter in filters)
                        query.Add(filter, BooleanClause.Occur.SHOULD);
                }
            }

            // apply date range filter ?
            foreach(string name in _before.Keys.Concat(_after.Keys)) {
                if ((_before.ContainsKey(name) && _before[name] != DateTime.MaxValue) || (_after.ContainsKey(name) && _after[name] != DateTime.MinValue)) {
                    var filter = new TermRangeQuery("date", 
                        DateTools.DateToString(_after.ContainsKey(name) ? _after[name] : DateTime.MinValue, DateTools.Resolution.SECOND),
                        DateTools.DateToString(_before.ContainsKey(name) ? _before[name] : DateTime.MaxValue, DateTools.Resolution.SECOND), 
                        true, true);
                    query.Add(filter, BooleanClause.Occur.MUST);
                }
            }

            if ( query.Clauses().Count == 0 ) { // get all documents ?
                query.Add(new TermRangeQuery("id", "0", "9", true, true), BooleanClause.Occur.SHOULD);
            }

            Logger.Debug("New search query: {0}", query.ToString());
            return query;
        }

        public IEnumerable<ISearchHit> Search() {
            var query = CreateQuery();

            var searcher = new IndexSearcher(_directory, true);

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

                searcher.Search(query, collector);

                var results = new List<DefaultSearchHit>();

                foreach ( var scoreDoc in collector.TopDocs().scoreDocs.Skip(_skip) ) {
                    results.Add(new DefaultSearchHit(searcher.Doc(scoreDoc.doc), scoreDoc.score));
                }

                Logger.Information("Search results: {0}", results.Count);
                return results;
            }
            finally {
                searcher.Close();
            }

        }

        public int Count() {
            var query = CreateQuery();

            var searcher = new IndexSearcher(_directory, true);
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
                    return new DefaultSearchHit(searcher.Doc(hits.scoreDocs[0].doc), hits.scoreDocs[0].score);
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
