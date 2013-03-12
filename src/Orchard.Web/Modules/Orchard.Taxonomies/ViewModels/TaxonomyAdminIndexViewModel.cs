using System.Collections.Generic;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.ViewModels {
    public class TaxonomyAdminIndexViewModel {
        public IList<TaxonomyEntry> Taxonomies { get; set; }
        public TaxonomiesAdminIndexBulkAction BulkAction { get; set; }
    }

    public class TaxonomyEntry {
        public TaxonomyPart Taxonomy { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum TaxonomiesAdminIndexBulkAction {
        None,
        Delete,
    }
}
