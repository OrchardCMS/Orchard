using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Handlers {

    public class WorkflowContentHandler : ContentHandler {
        // Used to memorize the ids of ContentItems for which we go through the
        // OnCreated handler.
        private List<int> _createdItems;
        // Used to memorize the ids of ContentItems for which we go through the
        // OnUpdated handler.
        private List<int> _updatedItems;

        public WorkflowContentHandler(IWorkflowManager workflowManager) {
            _createdItems = new List<int>();
            _updatedItems = new List<int>();

            OnPublished<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentPublished",
                    context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnUnpublished<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentUnpublished",
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
                (context, part) => {

                    workflowManager.TriggerEvent("ContentCreated", context.ContentItem,
                        () => new Dictionary<string, object> { { "Content", context.ContentItem } });

                    if (context.ContentItem != null) { // sanity check
                        _createdItems.Add(context.ContentItem.Id);
                    }
                });

            OnUpdated<ContentPart>(
                (context, part) => {
                    if(context.ContentItemRecord == null) {
                        return;
                    }

                    workflowManager.TriggerEvent(
                        "ContentUpdated",
                        context.ContentItem,
                        () => new Dictionary<string, object> { { "Content", context.ContentItem } }
                    );

                    if (context.ContentItem != null) { // sanity check
                        if (_createdItems.Contains(context.ContentItem.Id)) {
                            if (!_updatedItems.Contains(context.ContentItem.Id)) {
                                // first update after creation of item
                                workflowManager.TriggerEvent(
                                    "ContentFirstUpdated",
                                    context.ContentItem,
                                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }
                                );
                            }
                        }
                        // in case a further update is invoked, this would prevent
                        // the FirstUpdate event to be fired again
                        _updatedItems.Add(context.ContentItem.Id);
                    }
                });
        }
    }
}