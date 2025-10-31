using Lucene.Net.Analysis;

namespace Lucene.Services
{
    public class LuceneAnalyzerSelectorResult
    {
        public int Priority { get; set; }
        public Analyzer Analyzer { get; set; }
    }
}