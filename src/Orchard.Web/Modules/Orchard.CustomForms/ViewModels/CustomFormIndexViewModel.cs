using System.Collections.Generic;
using Orchard.CustomForms.Models;

namespace Orchard.CustomForms.ViewModels {

    public class CustomFormIndexViewModel {
        public IList<CustomFormEntry> CustomForms { get; set; }
        public CustomFormIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CustomFormEntry {
        public CustomFormPart CustomForm { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CustomFormIndexOptions {
        public string Search { get; set; }
        public CustomFormOrder Order { get; set; }
        public CustomFormFilter Filter { get; set; }
        public CustomFormBulkAction BulkAction { get; set; }
    }

    public enum CustomFormOrder {
        Name,
        Creation
    }

    public enum CustomFormFilter {
        All,
    }

    public enum CustomFormBulkAction {
        None,
        Publish,
        Unpublish,
        Delete
    }
}
