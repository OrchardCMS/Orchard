using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Services {
    public interface IWorkflowManager : IEventHandler {
        /// <summary>
        /// Triggers a specific Event, and provides the tokens context if the event is 
        /// actually executed
        /// </summary>
        /// <param name="name">The type of the event to trigger, e.g. Publish</param>
        /// <param name="target">The <see cref="IContent"/> content item the event is related to</param>
        /// <param name="tokensContext">An object containing the tokens context</param>
        void TriggerEvent(string name, IContent target, Func<Dictionary<string, object>> tokensContext);

        IEnumerable<ActivityRecord> ExecuteWorkflow(WorkflowDefinitionRecord workflowDefinitionRecord, ActivityRecord activityRecord, IContent target, Dictionary<string, object> tokens, dynamic workflowState);
    }

}