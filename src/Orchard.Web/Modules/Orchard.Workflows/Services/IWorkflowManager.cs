using System;
using System.Collections.Generic;
using Orchard.Events;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Services {
    public interface IWorkflowManager : IEventHandler {

        /// <summary>
        /// Triggers a specific Event, and provides the tokens context if the event is 
        /// actually executed
        /// </summary>
        /// <param name="name">The type of the event to trigger, e.g. Publish</param>
        /// <param name="tokensContext">An object containing the tokens context</param>
        void TriggerEvent(string name, Func<Dictionary<string, object>> tokensContext);

        ActivityRecord ExecuteWorkflow(WorkflowDefinitionRecord workflowDefinitionRecord, ActivityRecord activityRecord, Dictionary<string, object> tokens);
    }

}