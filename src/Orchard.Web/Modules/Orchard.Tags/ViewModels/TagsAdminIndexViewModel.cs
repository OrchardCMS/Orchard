using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Tags.Models;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminIndexViewModel : BaseViewModel {
        public IList<TagEntry> Tags { get; set; }
        public TagAdminIndexBulkAction BulkAction { get; set; }
    }

    public class TagEntry {
        public Tag Tag { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum TagAdminIndexBulkAction {
        None,
        Delete,
    }
}
