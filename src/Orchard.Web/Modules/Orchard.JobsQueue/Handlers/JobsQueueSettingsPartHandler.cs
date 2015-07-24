using Orchard.ContentManagement.Handlers;
using Orchard.JobsQueue.Models;

namespace Orchard.JobsQueue.Handlers {
    public class JobsQueueSettingsPartHandler : ContentHandler {
        public JobsQueueSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<JobsQueueSettingsPart>("Site"));
        }
    }
}