using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class MergeActivity : Task {

        public MergeActivity() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            // wait for all incoming branches to trigger the Execute before returning the result
            var branchesState = workflowContext.GetStateFor<string>(activityContext.Record, "Branches");

            if (String.IsNullOrWhiteSpace(branchesState)) {
                yield break;
            }
            
            var branches = GetBranches(branchesState);
            var inboundActivities = workflowContext.GetInboundTransitions(activityContext.Record);
            var done = inboundActivities
                .All(x => branches.Contains(GetTransitionKey(x)));

            if(done) {
                yield return T("Done");
            } 
        }

        public override string Name {
            get { return "MergeBranch"; }
        }

        public override LocalizedString Category {
            get { return T("Flow"); }
        }

        public override LocalizedString Description {
            get { return T("Merges multiple branches."); }
        }

        public override string Form {
            get { return null; }
        }

        public override void OnActivityExecuted(WorkflowContext workflowContext, ActivityContext activityContext) {

            // activity records pointed by the executed activity
            var outboundActivities = workflowContext.GetOutboundTransitions(activityContext.Record);

            // if a direct target of a Branch Activity is executed, then suppress all other direct waiting activities
            var childBranches = outboundActivities
                .Where(x => x.DestinationActivityRecord.Name == this.Name)
                .ToList();

            foreach (var childBranch in childBranches) {
                var branchesState = workflowContext.GetStateFor<string>(childBranch.DestinationActivityRecord, "Branches");
                var branches = GetBranches(branchesState);
                branches = branches.Union(new[] { GetTransitionKey(childBranch)}).Distinct();
                workflowContext.SetStateFor(childBranch.DestinationActivityRecord, "Branches", String.Join(",", branches.ToArray()));
            }
        }

        private string GetTransitionKey(TransitionRecord transitionRecord) {
            return "@" + transitionRecord.SourceActivityRecord.Id + "_" + transitionRecord.SourceEndpoint;
        }

        private IEnumerable<string> GetBranches(string branches) {
            if (String.IsNullOrEmpty(branches)) {
                return Enumerable.Empty<string>();
            }

            return branches.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        } 
    }


}