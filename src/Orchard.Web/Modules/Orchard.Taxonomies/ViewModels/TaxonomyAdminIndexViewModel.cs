using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Taxonomies.ViewModels {
    public class TaxonomyAdminIndexViewModel {
        public IList<TaxonomyEntry> Taxonomies { get; set; }
        public TaxonomiesAdminIndexBulkAction BulkAction { get; set; }
        public dynamic Pager { get; set; }
    }

    public class TaxonomyEntry {
        public int Id { get; set; }
        public bool IsInternal { get; set; }
        public string Name { get; set; }
        public ContentItem ContentItem { get; set; }
        public bool IsChecked { get; set; }
        public bool HasDraft { get; set; }
    }

    public enum TaxonomiesAdminIndexBulkAction {
        None,
        Delete,
    }
}
