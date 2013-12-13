using System;

namespace Orchard.Messaging.Models {
    public class MessagePriority {
        public virtual int Id { get; set; }
        public virtual int Value { get; set; }
        public virtual string Name { get; set; }
        public virtual string DisplayText { get; set; }
        public virtual bool Archived { get; set; }
        public virtual DateTime? ArchivedUtc { get; set; }

        public override string ToString() {
            return String.Format("{0} - {1}", Value, Name);
        }
    }
}