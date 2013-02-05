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
                    context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnRemoving<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentRemoved",
                    context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnVersioned<ContentPart>(
                (context, part1, part2) =>
                    workflowManager.TriggerEvent("ContentVersioned",
                    context.BuildingContentItem,
                    () => new Dictionary<string, object> { { "Content", context.BuildingContentItem } }));

            OnCreated<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentCreated",
                    context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

        }
    }
}