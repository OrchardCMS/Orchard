using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Standard;

namespace Lucene.Services {
    public class DefaultLuceneAnalyzerSelector : ILuceneAnalyzerSelector {
        public LuceneAnalyzerSelectorResult GetLuceneAnalyzer(string indexName) {
            return new LuceneAnalyzerSelectorResult {
                Priority = -5,
                Analyzer = new StandardAnalyzer(LuceneIndexProvider.LuceneVersion)
            };
        }

        public string Name { get { return "Default"; } }
    }
}