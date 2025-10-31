using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities
{
    public class TriggerActivity : Task
    {
        private readonly IWorkflowManager _workflowManager;

        public TriggerActivity(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var tokens = new Dictionary<string, object> { { "Content", workflowContext.Content }, { SignalActivity.SignalEventName, activityContext.GetState<string>(SignalActivity.SignalEventName) } };
            _workflowManager.TriggerEvent(SignalActivity.SignalEventName, workflowContext.Content, () => tokens);

            yield return T("Done");
        }

        public override string Name => "Trigger";

        public override LocalizedString Category => T("Events");

        public override LocalizedString Description => T("Triggers a Signal by its name.");

        public override string Form => "Trigger";
    }
}