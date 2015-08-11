using System;

namespace Orchard.Tasks.Locking.Records {
    public class DatabaseLockRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string MachineName { get; set; }
        public virtual DateTime? AcquiredUtc { get; set; }
    }
}