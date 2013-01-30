using System.Linq;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Activities {
    public class ExclusiveBranchActivity : BranchActivity {
        public override void OnActivityExecuted(WorkflowContext workflowContext, ActivityContext activityContext) {

            // for blocking activities only
            if (!activityContext.Activity.IsEvent) {
                return;
            }

            // activity records pointing to the executed activity
            var inboundActivities = workflowContext.GetInboundTransitions(activityContext.Record);


            // if a direct target of a Branch Activity is executed, then suppress all other direct waiting activities
            var parentBranchActivities = inboundActivities
                .Where(x => x.SourceActivityRecord.Name == typeof(ExclusiveBranchActivity).Name)
                .Select(x => x.SourceActivityRecord)
                .ToList();

            if (parentBranchActivities.Any()) {
                foreach (var parentBranch in parentBranchActivities) {
                    // remove all other waiting activities after the parent branch

                    var siblings = workflowContext.GetOutboundTransitions(parentBranch).Select(x => x.DestinationActivityRecord).ToList();
                    var awaitings = workflowContext.Record.AwaitingActivities.Where(x => siblings.Contains(x.ActivityRecord));
                    foreach (var awaiting in awaitings) {
                        workflowContext.Record.AwaitingActivities.Remove(awaiting);
                    }
                }
            }
        }
    }
}