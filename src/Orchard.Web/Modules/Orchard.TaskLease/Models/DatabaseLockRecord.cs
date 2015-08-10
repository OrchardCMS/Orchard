using System;

namespace Orchard.TaskLease.Models {
    public class DatabaseLockRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime? AcquiredUtc { get; set; }
    }
}