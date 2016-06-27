using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Tokens;

namespace Orchard.Workflows.Services {
    public class WorkflowManager : IWorkflowManager {
        private readonly IActivitiesManager _activitiesManager;
        private readonly IRepository<ActivityRecord> _activityRepository;
        private readonly IRepository<WorkflowRecord> _workflowRepository;
        private readonly IRepository<AwaitingActivityRecord> _awaitingActivityRepository;
        private readonly ITokenizer _tokenizer;

        public WorkflowManager(
            IActivitiesManager activitiesManager,
            IRepository<ActivityRecord> activityRepository,
            IRepository<WorkflowRecord> workflowRepository,
            IRepository<AwaitingActivityRecord> awaitingActivityRepository,
            ITokenizer tokenizer) {
            _activitiesManager = activitiesManager;
            _activityRepository = activityRepository;
            _workflowRepository = workflowRepository;
            _awaitingActivityRepository = awaitingActivityRepository;
            _tokenizer = tokenizer;

            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void TriggerEvent(string name, IContent target, Func<Dictionary<string, object>> tokensContext) {
            var tokens = tokensContext();

            var activity = _activitiesManager.GetActivityByName(name);

            if (activity == null) {
                Logger.Error("Activity {0} was not found", name);
                return;
            }
            
            var startedWorkflows = new List<ActivityRecord>();

            // look for workflow definitions with a corresponding starting activity
            // it's important to return activities at this point and not workflows,
            // as a workflow definition could have multiple entry points with the same type of activity
            startedWorkflows.AddRange(_activityRepository.Table.Where(
                x =>x.Name == name && x.Start && x.WorkflowDefinitionRecord.Enabled
                )
            );

            var awaitingActivities = new List<AwaitingActivityRecord>();

            // and any running workflow paused on this kind of activity for this content
            // it's important to return activities at this point as a workflow could be awaiting 
            // on several ones. When an activity is restarted, all the other ones of the same workflow are cancelled.
            var awaitingQuery = _awaitingActivityRepository.Table.Where(x => x.ActivityRecord.Name == name && x.ActivityRecord.Start == false);
            awaitingQuery = target == null || target.ContentItem == null
                    ? awaitingQuery.Where(x => x.WorkflowRecord.ContentItemRecord == null)
                    : awaitingQuery.Where(x => x.WorkflowRecord.ContentItemRecord == target.ContentItem.Record);
            awaitingActivities.AddRange(awaitingQuery.ToList());

            // if no activity record is matching the event, do nothing
            if (!startedWorkflows.Any() && !awaitingActivities.Any()) {
                return;
            }

            // resume halted workflows
            foreach (var awaitingActivityRecord in awaitingActivities) {
                var workflowContext = new WorkflowContext {
                    Content = target,
                    Tokens = tokens,
                    Record = awaitingActivityRecord.WorkflowRecord
                };

                workflowContext.Tokens["Workflow"] = workflowContext;

                var activityContext = CreateActivityContext(awaitingActivityRecord.ActivityRecord, tokens);

                // check the condition
                try {
                    if (!activity.CanExecute(workflowContext, activityContext)) {
                        continue;
                    }
                }
                catch (Exception e) {
                    Logger.Error("Error while evaluating an activity condition on {0}: {1}", name, e.ToString());
                    continue;
                }

                ResumeWorkflow(awaitingActivityRecord, workflowContext, tokens);
            }

            // start new workflows
            foreach (var activityRecord in startedWorkflows) {

                var workflowContext = new WorkflowContext {
                    Content = target,
                    Tokens = tokens,
                };

                workflowContext.Tokens["Workflow"] = workflowContext;

                var workflowRecord = new WorkflowRecord {
                    WorkflowDefinitionRecord = activityRecord.WorkflowDefinitionRecord,
                    State = "{}",
                    ContentItemRecord = workflowContext.Content == null || workflowContext.Content.ContentItem == null
                            ? null
                            : workflowContext.Content.ContentItem.Record
                };

                workflowContext.Record = workflowRecord;

                var activityContext = CreateActivityContext(activityRecord, tokens);

                // check the condition
                try {
                    if(!activity.CanExecute(workflowContext, activityContext)) {
                        continue;
                    }
                }
                catch (Exception e) {
                    Logger.Error("Error while evaluating an activity condition on {0}: {1}", name, e.ToString());
                    continue;
                }

                StartWorkflow(workflowContext, activityRecord, tokens);
            }
        }

        private ActivityContext CreateActivityContext(ActivityRecord activityRecord, IDictionary<string, object> tokens) {
            return new ActivityContext {
                Record = activityRecord,
                Activity = _activitiesManager.GetActivityByName(activityRecord.Name),
                State = new Lazy<dynamic>(() => GetState(activityRecord.State, tokens))
            };
        }

        private void StartWorkflow(WorkflowContext workflowContext, ActivityRecord activityRecord, IDictionary<string, object> tokens) {
            
            // signal every activity that the workflow is about to start
            var cancellationToken = new CancellationToken();
            InvokeActivities(activity => activity.OnWorkflowStarting(workflowContext, cancellationToken));

            if (cancellationToken.IsCancelled) {
                // workflow is aborted
                return;
            }

            // signal every activity that the workflow is has started
            InvokeActivities(activity => activity.OnWorkflowStarted(workflowContext));
            
            var blockedOn = ExecuteWorkflow(workflowContext, activityRecord, tokens).ToList();

            // is the workflow halted on a blocking activity ?
            if (!blockedOn.Any()) {
                // no, nothing to do
            }
            else {
                // workflow halted, create a workflow state
                _workflowRepository.Create(workflowContext.Record);

                foreach (var blocking in blockedOn) {
                    workflowContext.Record.AwaitingActivities.Add(new AwaitingActivityRecord {
                        ActivityRecord = blocking,
                        WorkflowRecord = workflowContext.Record
                    });
                }
            }
        }

        private void ResumeWorkflow(AwaitingActivityRecord awaitingActivityRecord, WorkflowContext workflowContext, IDictionary<string, object> tokens) {
            // signal every activity that the workflow is about to be resumed
            var cancellationToken = new CancellationToken();
            InvokeActivities(activity => activity.OnWorkflowResuming(workflowContext, cancellationToken));

            if (cancellationToken.IsCancelled) {
                // workflow is aborted
                return;
            }

            // signal every activity that the workflow is resumed
            InvokeActivities(activity => activity.OnWorkflowResumed(workflowContext));

            var workflow = awaitingActivityRecord.WorkflowRecord;
            workflowContext.Record = workflow;

            workflow.AwaitingActivities.Remove(awaitingActivityRecord);

            var blockedOn = ExecuteWorkflow(workflowContext, awaitingActivityRecord.ActivityRecord, tokens).ToList();

            // is the workflow halted on a blocking activity, and there is no more awaiting activities
            if (!blockedOn.Any() && !workflow.AwaitingActivities.Any()) {
                // no, delete the workflow
                _workflowRepository.Delete(awaitingActivityRecord.WorkflowRecord);
            }
            else {
                // add the new ones
                foreach (var blocking in blockedOn) {
                    workflow.AwaitingActivities.Add(new AwaitingActivityRecord {
                        ActivityRecord = blocking,
                        WorkflowRecord = workflow
                    });
                }
            }
        }

        public IEnumerable<ActivityRecord> ExecuteWorkflow(WorkflowContext workflowContext, ActivityRecord activityRecord, IDictionary<string, object> tokens) {
            var firstPass = true;
            var scheduled = new Stack<ActivityRecord>();

            scheduled.Push(activityRecord);

            var blocking = new List<ActivityRecord>();

            while (scheduled.Any()) {
                
                activityRecord = scheduled.Pop();

                var activityContext = CreateActivityContext(activityRecord, tokens);
                
                // while there is an activity to process

                if (!firstPass){
                    if (activityContext.Activity.IsEvent) {
                        blocking.Add(activityRecord);
                        continue;
                    }
                }
                else {
                    firstPass = false;    
                }

                // signal every activity that the activity is about to be executed
                var cancellationToken = new CancellationToken();
                InvokeActivities(activity => activity.OnActivityExecuting(workflowContext, activityContext, cancellationToken));

                if (cancellationToken.IsCancelled) {
                    // activity is aborted
                    continue;
                }

                var outcomes = activityContext.Activity.Execute(workflowContext, activityContext).ToList();

                // signal every activity that the activity is executed
                InvokeActivities(activity => activity.OnActivityExecuted(workflowContext, activityContext));

                foreach (var outcome in outcomes) {
                    // look for next activity in the graph
                    var transition = workflowContext.Record.WorkflowDefinitionRecord.TransitionRecords.FirstOrDefault(x => x.SourceActivityRecord == activityRecord && x.SourceEndpoint == outcome.TextHint);

                    if (transition != null) {
                        scheduled.Push(transition.DestinationActivityRecord);
                    }
                }
            }

            // apply Distinct() as two paths could block on the same activity
            return blocking.Distinct();
        }

        /// <summary>
        /// Executes a specific action on all the activities of a workflow, using a specific context
        /// </summary>
        private void InvokeActivities(Action<IActivity> action) {
            foreach (var activity in _activitiesManager.GetActivities()) {
                action(activity);
            }
        }

        private dynamic GetState(string state, IDictionary<string, object> tokens) {
            if (!String.IsNullOrWhiteSpace(state)) {
                var formatted = JsonConvert.DeserializeXNode(state, "Root").ToString();
                var tokenized = _tokenizer.Replace(formatted, tokens);
                var serialized = String.IsNullOrEmpty(tokenized) ? "{}" : JsonConvert.SerializeXNode(XElement.Parse(tokenized));
                return FormParametersHelper.FromJsonString(serialized).Root;
            }

            return FormParametersHelper.FromJsonString("{}");
        }
    }
}
