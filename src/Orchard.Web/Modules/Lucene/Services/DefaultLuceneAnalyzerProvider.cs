using Lucene.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Orchard;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace Lucene.Services {
    public class DefaultLuceneAnalyzerProvider : ILuceneAnalyzerProvider {
        private readonly IWorkContextAccessor _wca;
        private IEnumerable<ILuceneAnalyzerSelector> _analyzerSelectors;

        public DefaultLuceneAnalyzerProvider(IEnumerable<ILuceneAnalyzerSelector> analyzerSelectors, IWorkContextAccessor wca) {
            _analyzerSelectors = analyzerSelectors;
            _wca = wca;
        }

        public Analyzer GetAnalyzer(string indexName) {
            var luceneSettingsPart = _wca
                 .GetContext()
                 .CurrentSite
                 .As<LuceneSettingsPart>();
            if (luceneSettingsPart == null) {
                return new StandardAnalyzer(LuceneIndexProvider.LuceneVersion);
            }

            var currentIndexMapping = luceneSettingsPart
                .LuceneAnalyzerSelectorMappings
                .FirstOrDefault(mapping => mapping.IndexName == indexName);
            if (currentIndexMapping == null) {
                return new StandardAnalyzer(LuceneIndexProvider.LuceneVersion);
            }

            var analyzer = _analyzerSelectors
                .Where(x => x.Name == currentIndexMapping.AnalyzerName)
                .Select(x => x.GetLuceneAnalyzer(indexName))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority)
                .Select(x => x.Analyzer)
                .FirstOrDefault();

            return analyzer != null ? analyzer : new StandardAnalyzer(LuceneIndexProvider.LuceneVersion);
        }
    }
}
