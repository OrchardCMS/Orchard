using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Scheduling.Models {
    public class ScheduledAspect : ContentPart<ScheduledAspectRecord>, IScheduledAspect {
        public IScheduledTask Tasks {
            get { throw new NotImplementedException(); }
        }
    }

    public class ScheduledAspectRecord : ContentPartVersionRecord {
        public ScheduledAspectRecord() {
            Tasks = new List<ScheduledTaskRecord>();
        }

        public virtual IList<ScheduledTaskRecord> Tasks { get; set; }
    }

    public class ScheduledTaskRecord {
        public virtual int Id { get; set; }
        public virtual ScheduledAspectRecord ScheduledAspectRecord { get; set; }
        public virtual string Action { get; set; }
        public virtual DateTime? ScheduledUtc { get; set; }
    }
}
