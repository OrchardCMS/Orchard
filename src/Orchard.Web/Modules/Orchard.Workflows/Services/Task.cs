using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Services
{
    public abstract class Task : IActivity
    {

        public abstract string Name { get; }
        public abstract LocalizedString Category { get; }
        public abstract LocalizedString Description { get; }

        public virtual bool IsEvent => false;

        public bool CanStartWorkflow => false;

        public virtual string Form => null;

        public abstract IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext);

        public virtual bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public abstract IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext);

        public virtual void OnWorkflowStarting(WorkflowContext context, CancellationToken cancellationToken)
        {
        }

        public virtual void OnWorkflowStarted(WorkflowContext context)
        {
        }

        public virtual void OnWorkflowResuming(WorkflowContext context, CancellationToken cancellationToken)
        {
        }

        public virtual void OnWorkflowResumed(WorkflowContext context)
        {
        }

        public virtual void OnActivityExecuting(WorkflowContext workflowContext, ActivityContext activityContext, CancellationToken cancellationToken)
        {
        }

        public virtual void OnActivityExecuted(WorkflowContext workflowContext, ActivityContext activityContext)
        {
        }
    }
}