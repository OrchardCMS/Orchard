using System;
using Orchard.Data.Conventions;

namespace Orchard.Layouts.Models {
    public class ObjectStoreEntry {
        public virtual int Id { get; set; }
        public virtual string EntryKey { get; set; }
        [StringLengthMax]
        public virtual string Data { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual DateTime LastModifiedUtc { get; set; }
    }
}