using System.Collections.Generic;
using Orchard.Tags.Models;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminIndexViewModel {
        public IList<TagEntry> Tags { get; set; }
        public TagAdminIndexBulkAction BulkAction { get; set; }
    }

    public class TagEntry {
        public TagRecord Tag { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum TagAdminIndexBulkAction {
        None,
        Delete,
    }
}
