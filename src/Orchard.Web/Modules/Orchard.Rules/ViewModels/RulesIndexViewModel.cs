using System.Collections.Generic;
using Orchard.Rules.Models;

namespace Orchard.Rules.ViewModels {

    public class RulesIndexViewModel {
        public IList<RulesEntry> Rules { get; set; }
        public RulesIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class RulesEntry {
        public RuleRecord Rule { get; set; }
        
        public bool IsChecked { get; set; }
        public int RuleId { get; set; }
    }

    public class RulesIndexOptions {
        public string Search { get; set; }
        public RulesOrder Order { get; set; }
        public RulesFilter Filter { get; set; }
        public RulesBulkAction BulkAction { get; set; }
    }

    public enum RulesOrder {
        Name,
        Creation
    }

    public enum RulesFilter {
        All,
        Enabled,
        Disabled
    }

    public enum RulesBulkAction {
        None,
        Enable,
        Disable,
        Delete
    }
}
