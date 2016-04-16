using System;
using Orchard.ContentManagement.Records;

namespace Orchard.ImportExport.Models {
    public class ScheduledTaskRunHistory {
        public virtual int Id { get; set; }
        public virtual string ExecutionId { get; set; }
        public virtual DateTime? RunStartUtc { get; set; }
        public virtual DateTime? RunCompletedUtc { get; set; }
        public virtual RunStatus RunStatus { get; set; }

        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }

    public enum RunStatus {
        Started,
        Running,
        Success,
        Fail,
        Cancelled
    }
}