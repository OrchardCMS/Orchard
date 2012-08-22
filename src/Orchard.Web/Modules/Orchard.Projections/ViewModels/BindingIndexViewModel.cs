using System.Collections.Generic;
using Orchard.Projections.Models;

namespace Orchard.Projections.ViewModels {

    public class BindingIndexViewModel {
        public IList<BindingEntry> Bindings { get; set; }
        public BindingIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BindingEntry {
        public MemberBindingRecord Binding { get; set; }
        public bool IsChecked { get; set; }
    }

    public class BindingIndexOptions {
        public string Search { get; set; }
        public BindingsOrder Order { get; set; }
        public BindingsFilter Filter { get; set; }
        public BindingsBulkAction BulkAction { get; set; }
    }

    public enum BindingsOrder {
        Name,
        Creation
    }

    public enum BindingsFilter {
        All
    }

    public enum BindingsBulkAction {
        None,
        Delete
    }
}
