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

        private readonly Dictionary<string, BooleanClause[]> _fields;
        private int _count;
        private int _skip;
        private readonly Dictionary<string, DateTime> _before;
        private readonly Dictionary<string, DateTime> _after;
        private string _sort;
        private bool _sortDescending;
        private string _parse;
        private readonly Analyzer _analyzer;
        private string[] _defaultFields;

        // pending clause attributes
        private string _field;
        private string _terms;
        private BooleanClause.Occur _occur;
        private bool _prefix;
        private bool _stem;
        private float _boost;

        public ILogger Logger { get; set; }

        public DefaultSearchBuilder(Directory directory) {
            _directory = directory;
            Logger = NullLogger.Instance;

            _count = MaxResults;
            _skip = 0;
            _before = new Dictionary<string, DateTime>();
            _after = new Dictionary<string, DateTime>();
            _fields = new Dictionary<string, BooleanClause[]>();
            _sort = String.Empty;
            _sortDescending = true;
            _parse = String.Empty;
            _analyzer = DefaultIndexProvider.CreateAnalyzer();

            InitPendingClause();
        }

        public ISearchBuilder Parse(string[] defaultFields, string query) {
            if ( defaultFields.Length == 0 ) {
                throw new ArgumentException("Default field can't be empty");
            }

            if ( String.IsNullOrWhiteSpace(query) ) {
                throw new ArgumentException("Query can't be empty");
            }

            _defaultFields = defaultFields;
            _parse = query;
            return this;
        }

        public ISearchBuilder WithField(string field, string value) {
            CreatePendingClause();

            _field = field;
            _terms = value;

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
            _prefix = false;
            _stem = false;
            return this;
        }

        public ISearchBuilder Weighted(float weight) {
            _boost = weight;
            return this;
        }

        private void InitPendingClause() {
            _field = String.Empty;
            _terms = String.Empty;
            _occur = BooleanClause.Occur.SHOULD;
            _prefix = true;
            _stem = true;
            _boost = 0;
        }

        private void CreatePendingClause() {
            if(String.IsNullOrWhiteSpace(_field) || String.IsNullOrWhiteSpace(_terms)) {
                return;
            }

            var tokens = new List<string>();
            using ( var sr = new System.IO.StringReader(_terms) ) {
                var stream = _analyzer.TokenStream(_field, sr);
                
                if(_stem) {
                    stream = new PorterStemFilter(stream);
                }
                
                while ( stream.IncrementToken() ) {
                    tokens.Add(( (TermAttribute)stream.GetAttribute(typeof(TermAttribute)) ).Term());
                }
            }

            var clauses = tokens
                .Where(k => !String.IsNullOrWhiteSpace(k)) // remove empty strings
                .Select(QueryParser.Escape) // escape special chars (e.g. C#)
                .Select(k => new Term(_field, k)) // creates the Term instance
                .Select(t => _prefix ? new PrefixQuery(t) as Query : new TermQuery(t) as Query) // apply the corresponding Query
                .Select(q => {
                            if (_boost != 0) q.SetBoost(_boost);
                            return q;
                        })
                .Select(q => new BooleanClause(q, _occur)); // apply the corresponding clause

            if ( !_fields.ContainsKey(_field) ) {
                _fields[_field] = new BooleanClause[0];
            }

            _fields[_field] = _fields[_field].Union(clauses).ToArray();
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
            CreatePendingClause();

            var query = new BooleanQuery();

            if(!String.IsNullOrWhiteSpace(_parse)) {
                
                 foreach ( var defaultField in _defaultFields ) {
                     var clause = new BooleanClause(new QueryParser(DefaultIndexProvider.LuceneVersion, defaultField, DefaultIndexProvider.CreateAnalyzer()).Parse(_parse), BooleanClause.Occur.SHOULD);   
                     query.Add(clause);
                }
            }


            if ( _fields.Keys.Count > 0 ) {  // apply specific filters if defined
                foreach ( var clauses in _fields.Values ) {
                    foreach( var clause in clauses)
                        query.Add(clause);
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
