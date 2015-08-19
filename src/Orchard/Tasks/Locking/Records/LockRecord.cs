using System;

namespace Orchard.Tasks.Locking.Records {
    public class LockRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Owner { get; set; }
        public virtual int ReferenceCount { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual DateTime ValidUntilUtc { get; set; }
    }
}