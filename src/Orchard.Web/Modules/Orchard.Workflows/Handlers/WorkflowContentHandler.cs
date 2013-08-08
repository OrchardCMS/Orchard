using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Handlers {

    public class WorkflowContentHandler : ContentHandler {
        private readonly HashSet<int> _contentCreated = new HashSet<int>();

        public WorkflowContentHandler(IWorkflowManager workflowManager) {

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

            OnUpdated<ContentPart>(
                (context, part) => {
                    workflowManager.TriggerEvent("ContentUpdated",
                        context.ContentItem,
                        () => new Dictionary<string, object> { { "Content", context.ContentItem } });

                    // Trigger the ContentCreated event only when its values have been updated
                    if(_contentCreated.Contains(context.ContentItem.Id)) {
                        workflowManager.TriggerEvent("ContentCreated",
                            context.ContentItem,
                            () => new Dictionary<string, object> {{"Content", context.ContentItem}});
                    }
                });

            OnCreated<ContentPart>(
                // Flag the content item as "just created" but actually trigger the event
                // when its content has been updated as it is what users would expect.
                (context, part) => _contentCreated.Add(context.ContentItem.Id)
            );

        }
    }
}