using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Orchard;

namespace Lucene.Services {
    public interface ILuceneAnalyzerProvider : IDependency {
        Analyzer GetAnalyzer(string indexName);
    }
}
