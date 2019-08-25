using Lucene.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System.Collections.Generic;

namespace Lucene.Models {
    public class LuceneSettingsPart : ContentPart {
        public string LuceneAnalyzerSelectorMappingsSerialized {
            get { return this.Retrieve(x => x.LuceneAnalyzerSelectorMappingsSerialized); }
            set { this.Store(x => x.LuceneAnalyzerSelectorMappingsSerialized, value); }
        }

        private readonly LazyField<IEnumerable<LuceneAnalyzerSelectorMapping>> _luceneAnalyzerSelectorMappings = new LazyField<IEnumerable<LuceneAnalyzerSelectorMapping>>();
        internal LazyField<IEnumerable<LuceneAnalyzerSelectorMapping>> LuceneAnalyzerSelectorMappingsField { get { return _luceneAnalyzerSelectorMappings; }
        }
        public IEnumerable<LuceneAnalyzerSelectorMapping> LuceneAnalyzerSelectorMappings {
            get { return _luceneAnalyzerSelectorMappings.Value; }
            set { _luceneAnalyzerSelectorMappings.Value = value; }
        }
    }
}