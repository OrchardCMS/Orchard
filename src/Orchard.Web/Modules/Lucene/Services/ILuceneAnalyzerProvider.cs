using Lucene.Net.Analysis;
using Orchard;

namespace Lucene.Services
{
    public interface ILuceneAnalyzerProvider : IDependency
    {
        Analyzer GetAnalyzer(string indexName);
    }
}
