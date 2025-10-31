using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Lucene.Models
{
    public class LuceneSettingsPart : ContentPart
    {
        public string LuceneAnalyzerSelectorMappingsSerialized
        {
            get { return this.Retrieve(x => x.LuceneAnalyzerSelectorMappingsSerialized); }
            set { this.Store(x => x.LuceneAnalyzerSelectorMappingsSerialized, value); }
        }

        internal LazyField<IEnumerable<LuceneAnalyzerSelectorMapping>> LuceneAnalyzerSelectorMappingsField { get; } = new LazyField<IEnumerable<LuceneAnalyzerSelectorMapping>>();
        public IEnumerable<LuceneAnalyzerSelectorMapping> LuceneAnalyzerSelectorMappings
        {
            get { return LuceneAnalyzerSelectorMappingsField.Value; }
            set { LuceneAnalyzerSelectorMappingsField.Value = value; }
        }
    }
}