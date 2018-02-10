using System.Collections.Generic;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Settings;

namespace Orchard.Taxonomies.ViewModels {
    public class TaxonomyFieldViewModel {
        public int TaxonomyId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public TaxonomyFieldSettings Settings { get; set; }
        public IList<TermEntry> Terms { get; set; }
        public IEnumerable<TermPart> SelectedTerms { get; set; }
        public int SingleTermId { get; set; }
        public bool HasTerms { get; set; }
    }
}