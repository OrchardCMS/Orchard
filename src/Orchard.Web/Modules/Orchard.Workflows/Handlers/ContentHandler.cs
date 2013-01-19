using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Handlers {

    public class RulePartHandler : ContentHandler {
        public RulePartHandler(IWorkflowManager workflowManager) {

            OnPublished<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentPublished",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnRemoved<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentRemoved",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnVersioned<ContentPart>(
                (context, part1, part2) =>
                    workflowManager.TriggerEvent("ContentVersioned",
                    () => new Dictionary<string, object> { { "Content", part1.ContentItem } }));

            OnCreated<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentCreated",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

        }
    }
}