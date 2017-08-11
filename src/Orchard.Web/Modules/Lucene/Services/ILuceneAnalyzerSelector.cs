using Orchard;

namespace Lucene.Services {
    public interface ILuceneAnalyzerSelector : IDependency {
        LuceneAnalyzerSelectorResult GetLuceneAnalyzer(string indexName);
        string Name { get; }
    }
}