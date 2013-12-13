using System;
using Orchard.Data.Conventions;

namespace Orchard.Messaging.Models {
    public class QueuedMessageRecord {
        public virtual int Id { get; set; }
        public virtual int QueueId { get; set; }
        public virtual MessagePriority Priority { get; set; }
        public virtual string ChannelName { get; set; }

        [StringLengthMax]
        public virtual string Recipients { get; set; }

        [StringLengthMax]
        public virtual string Payload { get; set; }

        public virtual QueuedMessageStatus Status { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual DateTime? StartedUtc { get; set; }
        public virtual DateTime? CompletedUtc { get; set; }

        [StringLengthMax]
        public virtual string Result { get; set; }
    }
}