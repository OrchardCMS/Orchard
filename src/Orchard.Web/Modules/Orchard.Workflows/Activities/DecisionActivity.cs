using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models.Descriptors;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class DecisionActivity : BaseActivity {

        public DecisionActivity() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "Decision"; }
        }

        public override LocalizedString Category {
            get { return T("Misc"); }
        }

        public override LocalizedString Description {
            get { return T("Evaluates an expression.");  }
        }
        
        public override IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context) {
            return new[] { T("True"), T("False") };
        }

        public override LocalizedString Execute(ActivityContext context) {
            return T("True");
        }
    }
}