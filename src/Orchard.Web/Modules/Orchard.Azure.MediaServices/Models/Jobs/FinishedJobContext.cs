using Orchard.Azure.MediaServices.Models.Records;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace Orchard.Azure.MediaServices.Models.Jobs {
    public class FinishedJobContext {
        public CloudVideoPart CloudVideoPart { get; set; }
        public JobRecord JobRecord { get; set; }
        public IJob Job { get; set; }
    }
}