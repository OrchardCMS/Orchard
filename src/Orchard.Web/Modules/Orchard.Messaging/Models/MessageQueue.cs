using System;

namespace Orchard.Messaging.Models {
    public class MessageQueue {
        public MessageQueue(MessageQueueRecord record) {
            Record = record;
        }

        public MessageQueueRecord Record { get; private set; }

        public int Id {
            get { return Record.Id; }
        }

        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        public MessageQueueStatus Status {
            get { return Record.Status; }
            internal set { Record.Status = value; }
        }

        public DateTime? StartedUtc {
            get { return Record.StartedUtc; }
            internal set { Record.StartedUtc = value; }
        }

        public DateTime? EndedUtc {
            get { return Record.EndedUtc; }
            internal set { Record.EndedUtc = value; }
        }

        public override string ToString() {
            return String.Format("{0} - {1}", Name, Status);
        }
    }
}