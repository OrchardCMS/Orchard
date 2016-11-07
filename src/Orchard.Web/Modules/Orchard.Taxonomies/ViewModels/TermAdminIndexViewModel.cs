using System.Collections.Generic;
using Orchard.Taxonomies.Models;
using Orchard.ContentManagement;

namespace Orchard.Taxonomies.ViewModels {
    public class TermAdminIndexViewModel {
        public IList<TermEntry> Terms { get; set; }
        public TermsAdminIndexBulkAction BulkAction { get; set; }
        public TaxonomyPart Taxonomy { get; set; }
        public int TaxonomyId { get; set; }
        public dynamic Pager { get; set; }
    }

    public class TermEntry {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Selectable { get; set; }
        public int Count { get; set; }
        public int Weight { get; set; }
        public bool IsChecked { get; set; }
        public ContentItem ContentItem { get; set; }
    }

    public enum TermsAdminIndexBulkAction {
        None,
        Delete,
        Merge,
        Move,
    }
}
