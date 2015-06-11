using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.JobsQueue.Models;

namespace Orchard.JobsQueue.Handlers {
    [UsedImplicitly]
    public class JobsQueueSettingsPartHandler : ContentHandler {
        public JobsQueueSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<JobsQueueSettingsPart>("Site"));
        }
    }
}