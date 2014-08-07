using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;

namespace Lucene.Services {
    public interface ILuceneAnalyzerSelector : IDependency {
        LuceneAnalyzerSelectorResult GetLuceneAnalyzer(string indexName);
    }
}