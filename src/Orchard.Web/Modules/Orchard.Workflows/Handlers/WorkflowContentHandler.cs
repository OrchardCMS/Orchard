using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Handlers {

    public class WorkflowContentHandler : ContentHandler {
        // Used to memorize the ids of ContentItems for which we go through the
        // OnCreated handler.
        private HashSet<int> _createdItems;
        // Used to memorize the ids of ContentItems for which we go through the
        // OnUpdated handler.
        private HashSet<int> _updatedItems;

        public WorkflowContentHandler(IWorkflowManager workflowManager) {
            _createdItems = new HashSet<int>();
            _updatedItems = new HashSet<int>();

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

                    if (context.ContentItem != null) { // sanity check
                        _createdItems.Add(context.ContentItem.Id);
                    }

                    workflowManager.TriggerEvent("ContentCreated", context.ContentItem,
                        () => new Dictionary<string, object> { { "Content", context.ContentItem } });
                });

            OnUpdated<ContentPart>(
                (context, part) => {
                    if(context.ContentItemRecord == null) {
                        return;
                    }

                    if (context.ContentItem != null) { // sanity check
                        if (!_updatedItems.Contains(context.ContentItem.Id)) {
                            // in case a further update is invoked, this would prevent
                            // the FirstUpdate event to be fired again
                            _updatedItems.Add(context.ContentItem.Id);
                            if (_createdItems.Contains(context.ContentItem.Id)) {
                                // first update after creation of item
                                workflowManager.TriggerEvent(
                                    "ContentFirstUpdated",
                                    context.ContentItem,
                                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }
                                );
                            }
                        }
                    }

                    workflowManager.TriggerEvent(
                        "ContentUpdated",
                        context.ContentItem,
                        () => new Dictionary<string, object> { { "Content", context.ContentItem } }
                    );

                });
        }
    }
}