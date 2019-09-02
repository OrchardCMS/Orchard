using System.Collections.Generic;
using Orchard.Indexing.Services;

namespace Orchard.Indexing.ViewModels {
    public class IndexViewModel {
        public IIndexProvider IndexProvider { get; set; }
        public IEnumerable<IndexEntry> IndexEntries { get; set;}
    }
}