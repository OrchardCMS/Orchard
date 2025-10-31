using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities
{
    /// <summary>
    /// Represents a named event which can be triggered by any kind of activity.
    /// </summary>
    public class SignalActivity : Event
    {
        public const string SignalEventName = "Signal";

        public SignalActivity()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return activityContext.GetState<string>(SignalEventName) == workflowContext.Tokens[SignalEventName].ToString();
        }

        public override bool CanStartWorkflow => true;

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override string Name => SignalEventName;

        public override LocalizedString Category => T("Events");

        public override LocalizedString Description => T("Suspends the workflow until this signal is specifically triggered.");

        public override string Form => "SignalEvent";
    }
}