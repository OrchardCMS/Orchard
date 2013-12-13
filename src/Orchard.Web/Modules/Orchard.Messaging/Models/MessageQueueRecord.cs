using System;

namespace Orchard.Messaging.Models {
    public class MessageQueueRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual MessageQueueStatus Status { get; set; }
        public virtual DateTime? StartedUtc { get; set; }
        public virtual DateTime? EndedUtc { get; set; }

        public override string ToString() {
            return String.Format("{0} - {1}", Name, Status);
        }
    }
}