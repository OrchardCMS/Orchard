using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Handlers {

    public class WorkflowHandler : ContentHandler {

        public WorkflowHandler(
            IRepository<AwaitingActivityRecord> awaitingActivityRepository,
            IRepository<WorkflowRecord> workflowRepository
            ) {

            // Delete any pending workflow related to a deleted content item
            OnRemoving<ContentPart>(
                (context, part) => {
                    var awaiting = awaitingActivityRepository.Table.Where(x => x.ContentItemRecord == context.ContentItemRecord).ToList();

                    foreach (var item in awaiting) {
                        workflowRepository.Delete(item.WorkflowRecord);
                    }
                });
        }
    }
}