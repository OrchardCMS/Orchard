using System.Collections.Generic;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Services {
    public interface IMessageQueueService : IDependency {
        QueuedMessageRecord Enqueue(string type, object payload, int priority);
        QueuedMessageRecord GetMessage(int id);
        IEnumerable<QueuedMessageRecord> GetMessages(QueuedMessageStatus? status, int startIndex, int count);
        int GetMessagesCount(QueuedMessageStatus? status = null);
        void Resume();
        void Pause();
    }
}