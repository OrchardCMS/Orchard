using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Orchard;

namespace Lucene.Services {
    public class DefaultLuceneAnalyzerProvider : ILuceneAnalyzerProvider {

        private IEnumerable<ILuceneAnalyzerSelector> _analyzerSelectors;

        public DefaultLuceneAnalyzerProvider(IEnumerable<ILuceneAnalyzerSelector> analyzerSelectors) {
            _analyzerSelectors = analyzerSelectors;
        }

        public Analyzer GetAnalyzer(string indexName) {
            var analyzer = _analyzerSelectors
                .Select(x => x.GetLuceneAnalyzer(indexName))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority)
                .Select(x => x.Analyzer)
                .FirstOrDefault();

            if (analyzer != null) {
                return analyzer;
            }

            return new StandardAnalyzer(LuceneIndexProvider.LuceneVersion);
        }
    }
}
