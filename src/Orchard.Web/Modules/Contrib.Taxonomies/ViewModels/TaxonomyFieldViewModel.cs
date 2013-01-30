using System.Collections.Generic;
using Contrib.Taxonomies.Settings;

namespace Contrib.Taxonomies.ViewModels {
    public class TaxonomyFieldViewModel {
        public string Name { get; set; }
        public TaxonomyFieldSettings Settings { get; set; }
        public IList<TermEntry> Terms { get; set; }
        public int SingleTermId { get; set; }
    }
}