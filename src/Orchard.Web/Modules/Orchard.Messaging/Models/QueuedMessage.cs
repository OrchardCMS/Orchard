using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Orchard.Messaging.Services;

namespace Orchard.Messaging.Models {
    public class QueuedMessage {
        // ReSharper disable InconsistentNaming
        public Lazy<MessageQueue> QueueField;
        public Lazy<IMessageChannel> ChannelField;
        public Lazy<IEnumerable<MessageRecipient>> RecipientsField;
        // ReSharper restore InconsistentNaming

        public QueuedMessage(QueuedMessageRecord record) {
            Record = record;
        }

        public QueuedMessageRecord Record {
            get; private set;
        }

        public int Id {
            get { return Record.Id; }
        }
        
        public MessagePriority Priority {
            get { return Record.Priority; }
            set { Record.Priority = value; }
        }

        public QueuedMessageStatus Status {
            get { return Record.Status; }
            internal set { Record.Status = value; }
        }

        public string Result {
            get { return Record.Result; }
            set { Record.Result = value; }
        }

        public DateTime CreatedUtc {
            get { return Record.CreatedUtc; }
        }

        public DateTime? StartedUtc {
            get { return Record.StartedUtc; }
            internal set { Record.StartedUtc = value; }
        }

        public DateTime? CompletedUtc {
            get { return Record.CompletedUtc; }
            internal set { Record.CompletedUtc = value; }
        }

        public MessageQueue Queue {
            get { return QueueField.Value; }
        }

        public IMessageChannel Channel {
            get { return ChannelField.Value; }
        }

        public IEnumerable<MessageRecipient> Recipients {
            get { return RecipientsField.Value; }
        }

        public T GetPayload<T>() {
            return Record.Payload != null ? JsonConvert.DeserializeObject<T>(Record.Payload) : default(T);
        }

        public void SetPayload<T>(T value) {
            Record.Payload = !ReferenceEquals(value, default(T)) ? JsonConvert.SerializeObject(value) : null;
        }

        public override string ToString() {
            return String.Format("Recipients: {0}", String.Join(", ", Recipients.Select(x => x.ToString())));
        }
    }
}