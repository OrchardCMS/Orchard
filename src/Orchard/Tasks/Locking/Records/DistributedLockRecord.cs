using System;

namespace Orchard.Tasks.Locking.Records {
    public class DistributedLockRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string MachineName { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual DateTime? ValidUntilUtc { get; set; }
    }
}