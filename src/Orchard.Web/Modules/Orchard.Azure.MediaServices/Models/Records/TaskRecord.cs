using Orchard.Azure.MediaServices.Models.Jobs;
using Orchard.Data.Conventions;

namespace Orchard.Azure.MediaServices.Models.Records {
    public class TaskRecord {
        public virtual int Id { get; set; }
        public virtual JobRecord Job { get; set; }
        public virtual string WamsTaskId { get; set; }
        public virtual string TaskProviderName { get; set; }
        public virtual int TaskIndex { get; set; }
        public virtual JobStatus Status { get; set; }
        public virtual int PercentComplete { get; set; }

        [StringLengthMax]
        public virtual string SettingsXml { get; set; }

        public virtual string HarvestAssetType { get; set; }
        public virtual string HarvestAssetName { get; set; }
    }
}
