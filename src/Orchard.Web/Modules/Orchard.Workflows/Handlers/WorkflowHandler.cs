using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Handlers {

    public class WorkflowHandler : ContentHandler {

        public WorkflowHandler(
            IRepository<WorkflowRecord> workflowRepository
            ) {

            // Delete any pending workflow related to a deleted content item
            OnRemoving<ContentPart>(
                (context, part) => {
                    var workflows = workflowRepository.Table.Where(x => x.ContentItemRecord == context.ContentItemRecord).ToList();

                    foreach (var item in workflows) {
                        workflowRepository.Delete(item);
                    }
                });
        }
    }
}