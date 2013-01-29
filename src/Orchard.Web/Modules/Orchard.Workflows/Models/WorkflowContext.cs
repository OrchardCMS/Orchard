using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Models {
    public class WorkflowContext {
        public WorkflowContext() {
            Tokens = new Dictionary<string, object>();
        }

        /// <summary>
        /// If set, represents the subject of the current workflow
        /// </summary>
        public IContent Content { get; set; }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
        public dynamic WorkflowState { get; set; }

        public IActivity Activity { get; set; }
        public ActivityRecord Record { get; set; }

        /// <summary>
        /// Schedules a specific 
        /// </summary>
        public Action<ActivityRecord> Schedule { get; set; }
        public IEnumerable<TransitionRecord> GetInboundTransitions(ActivityRecord activityRecord) {
            return Record.WorkflowDefinitionRecord
                .TransitionRecords
                .Where(transition => 
                    transition.DestinationActivityRecord == activityRecord
                ).ToArray();
        }

        public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord) {
            return Record.WorkflowDefinitionRecord
                .TransitionRecords
                .Where(transition => 
                    transition.SourceActivityRecord == activityRecord
                ).ToArray();
        }

        public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord, LocalizedString outcome) {
            return Record.WorkflowDefinitionRecord
                .TransitionRecords
                .Where(transition =>
                    transition.SourceActivityRecord == activityRecord 
                    && transition.SourceEndpoint == outcome.TextHint
                ).ToArray();
        }

    }
}