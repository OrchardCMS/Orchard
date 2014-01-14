using Orchard.ContentManagement;

namespace Orchard.JobsQueue.Models {
    public class JobsQueueSettingsPart : ContentPart {

        public JobsQueueStatus Status {
            get { return this.Retrieve(x => x.Status); }
            set { this.Store(x => x.Status, value); }
        }
    }
}
