using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class DecisionActivity : Task {

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
        
        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext context) {
            return new[] { T("True"), T("False") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext context) {
            yield return T("True");
        }
    }
}