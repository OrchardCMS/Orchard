using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Services {
    public abstract class Task : IActivity {

        public abstract string Name { get; }
        public abstract LocalizedString Category { get; }
        public abstract LocalizedString Description { get; }

        public virtual bool IsEvent {
            get { return false; }
        }

        public bool CanStartWorkflow { get { return false; } }

        public virtual string Form {
            get { return null; }
        }

        public abstract IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext context);

        public virtual bool CanExecute(WorkflowContext context) {
            return true;
        }

        public abstract IEnumerable<LocalizedString> Execute(WorkflowContext context);

        public virtual void OnWorkflowStarting(WorkflowContext context, CancellationToken cancellationToken) {
        }

        public virtual void OnWorkflowStarted(WorkflowContext context) {
        }

        public virtual void OnWorkflowResuming(WorkflowContext context, CancellationToken cancellationToken) {
        }

        public virtual void OnWorkflowResumed(WorkflowContext context) {
        }

        public virtual void OnActivityExecuting(WorkflowContext context, CancellationToken cancellationToken) {
        }

        public virtual void OnActivityExecuted(WorkflowContext context) {
        }
    }
}