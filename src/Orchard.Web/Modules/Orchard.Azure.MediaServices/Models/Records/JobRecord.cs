using System;
using System.Collections.Generic;
using Orchard.Azure.MediaServices.Models.Jobs;
using Orchard.Data.Conventions;

namespace Orchard.Azure.MediaServices.Models.Records {
    public class JobRecord {

        public JobRecord() {
            Tasks = new List<TaskRecord>();
        }
        public virtual int Id { get; set; }
        public virtual int CloudVideoPartId { get; set; }
        public virtual string WamsJobId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual JobStatus Status { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? StartedUtc { get; set; }
        public virtual DateTime? FinishedUtc { get; set; }
        public virtual string ErrorMessage { get; set; }
        public virtual string OutputAssetName { get; set; }
        public virtual string OutputAssetDescription { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<TaskRecord> Tasks { get; set; }
    }
}
