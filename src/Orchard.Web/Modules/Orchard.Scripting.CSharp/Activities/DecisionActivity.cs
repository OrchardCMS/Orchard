using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Scripting.CSharp.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Scripting.CSharp.Activities {
    public class DecisionActivity : Task {
        private readonly ICSharpService _csharpService;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;

        public DecisionActivity(
            IOrchardServices orchardServices,
            ICSharpService csharpService,
            IWorkContextAccessor workContextAccessor) {
            _csharpService = csharpService;
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
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
            get { return T("Evaluates an expression."); }
        }

        public override string Form {
            get { return "ActivityActionDecision"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return GetOutcomes(activityContext).Select(outcome => T(outcome));
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var script = activityContext.GetState<string>("Script");
            object outcome = null;

            // Start the script with the new token syntax.
            script = "// #{ }" + System.Environment.NewLine + script;

            if (workflowContext.Content != null)
                _csharpService.SetParameter("ContentItem", (dynamic)workflowContext.Content.ContentItem);

            _csharpService.SetParameter("Services", _orchardServices);
            _csharpService.SetParameter("WorkContext", _workContextAccessor.GetContext());
            _csharpService.SetParameter("Workflow", workflowContext);
            _csharpService.SetFunction("T", (Func<string, string>)(x => T(x).Text));
            _csharpService.SetFunction("SetOutcome", (Action<object>)(x => outcome = x));

            _csharpService.Run(script);

            yield return T(Convert.ToString(outcome));
        }

        private IEnumerable<string> GetOutcomes(ActivityContext context) {

            var outcomes = context.GetState<string>("Outcomes");

            if (String.IsNullOrEmpty(outcomes)) {
                return Enumerable.Empty<string>();
            }

            return outcomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

        }
    }
}