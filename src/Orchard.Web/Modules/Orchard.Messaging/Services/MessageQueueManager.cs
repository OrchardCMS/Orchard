using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Messaging.Models;
using Orchard.Services;

namespace Orchard.Messaging.Services {
    public interface IMessageQueueManager : IDependency {
        QueuedMessage Send<T>(MessageRecipient recipient, string channelName, T payload, MessagePriority priority = null, int? queueId = null);
        QueuedMessage Send<T>(IEnumerable<MessageRecipient> recipients, string channelName, T payload, MessagePriority priority = null, int? queueId = null);
        QueuedMessage Send<T>(string recipient, string channelName, T payload, MessagePriority priority = null, int? queueId = null);
        QueuedMessage Send<T>(IEnumerable<string> recipients, string channelName, T payload, MessagePriority priority = null, int? queueId = null);
        MessageQueue GetQueue(int id);
        MessageQueue GetDefaultQueue();
        MessagePriority GetPriority(int id);
        MessagePriority GetPriority(string name);
        IEnumerable<MessagePriority> GetPriorities();
        MessagePriority GetDefaultPriority();
        IEnumerable<MessagePriority> CreateDefaultPriorities();
        void DeletePriority(MessagePriority priority);
        IEnumerable<MessageQueue> GetIdleQueues();
        void EnterProcessingStatus(MessageQueue queue);
        void ExitProcessingStatus(MessageQueue queue);
        IEnumerable<MessageQueue> GetQueues();
        int CountMessages(int queueId, QueuedMessageStatus? status = null);
        IQueryable<QueuedMessage> GetMessages(int queueId, QueuedMessageStatus? status = null, int startIndex = 0, int pageSize = 10);
        IEnumerable<QueuedMessage> GetPendingMessages(int queueId, int pageSize = 10);
        QueuedMessage GetMessage(int id);
        MessageQueue CreateQueue();
        MessageQueue CreateDefaultQueue();
        IMessageChannel GetChannel(string name);
        IEnumerable<IMessageChannel> GetChannels();
        void Resume(MessageQueue queue);
        void Pause(MessageQueue queue);
        MessagePriority CreatePriority(string name, string displayText, int rank);
    }

    [OrchardFeature("Orchard.Messaging.Queuing")]
    public class MessageQueueManager : IMessageQueueManager {
        private readonly IClock _clock;
        private readonly IRepository<MessageQueueRecord> _queueRepository;
        private readonly IRepository<QueuedMessageRecord> _messageRepository;
        private readonly IRepository<MessagePriority> _priorityRepository;

        public MessageQueueManager(
            IClock clock, 
            IRepository<MessageQueueRecord> queueRepository, 
            IRepository<QueuedMessageRecord> messageRepository, 
            IRepository<MessagePriority> priorityRepository, 
            IEnumerable<IMessageChannel> channels) {
            _clock = clock;
            _queueRepository = queueRepository;
            _messageRepository = messageRepository;
            _priorityRepository = priorityRepository;
            ChannelsDictionary = channels.ToDictionary(x => x.Name);
        }

        public QueuedMessage Send<T>(MessageRecipient recipient, string channelName, T payload, MessagePriority priority = null, int? queueId = null) {
            return Send(new[] {recipient}, channelName, payload, priority, queueId);
        }

        public QueuedMessage Send<T>(string recipient, string channelName, T payload, MessagePriority priority = null, int? queueId = null) {
            return Send(new[] { recipient }, channelName, payload, priority, queueId);
        }

        public QueuedMessage Send<T>(IEnumerable<string> recipients, string channelName, T payload, MessagePriority priority = null, int? queueId = null) {
            return Send(recipients.Select(x => new MessageRecipient(x)), channelName, payload, priority, queueId);
        }

        public QueuedMessage Send<T>(IEnumerable<MessageRecipient> recipients, string channelName, T payload, MessagePriority priority = null, int? queueId = null) {
            var queue = queueId != null ? GetQueue(queueId.Value) ?? GetDefaultQueue() : GetDefaultQueue();

            var queuedMessage = new QueuedMessageRecord {
                Payload = ToJson(payload),
                Recipients = ToJson(recipients.ToList()),
                ChannelName = channelName,
                Priority = priority ?? GetDefaultPriority(),
                QueueId = queue.Id,
                CreatedUtc = _clock.UtcNow,
                Status = QueuedMessageStatus.Pending
            };

            _messageRepository.Create(queuedMessage);

            return ActivateMessage(queuedMessage);
        }

        public IMessageChannel GetChannel(string name) {
            return ChannelsDictionary[name];
        }

        public IEnumerable<IMessageChannel> GetChannels() {
            return ChannelsDictionary.Select(x => x.Value);
        }

        public void Resume(MessageQueue queue) {
            if(queue.Status != MessageQueueStatus.Paused)
                throw new InvalidOperationException("Cannot resume a queue that is not paused.");

            queue.Status = MessageQueueStatus.Idle;
        }

        public void Pause(MessageQueue queue) {
            if (queue.Status == MessageQueueStatus.Paused)
                throw new InvalidOperationException("Cannot resume a queue that is already paused.");

            queue.Status = MessageQueueStatus.Paused;
        }

        public MessagePriority CreatePriority(string name, string displayText, int value) {
            var priority = new MessagePriority {
                Name = name,
                DisplayText = displayText,
                Value = value
            };
            _priorityRepository.Create(priority);
            return priority;
        }

        public IDictionary<string, IMessageChannel> ChannelsDictionary { get; private set; }

        public MessageQueue GetDefaultQueue() {
            return ActivateQueue(_queueRepository.Table.FirstOrDefault() ?? CreateDefaultQueue());
        }

        public MessagePriority GetPriority(int id) {
            return _priorityRepository.Get(id);
        }

        public MessagePriority GetPriority(string name) {
            return _priorityRepository.Get(x => x.Name == name);
        }

        public IEnumerable<MessagePriority> GetPriorities() {
            return _priorityRepository.Table.Where(x => !x.Archived).OrderBy(x => x.Value).ToList();
        }

        public MessageQueue GetQueue(int id) {
            var record = _queueRepository.Get(id);
            return record != null ? ActivateQueue(record) : null;
        }

        public MessagePriority GetDefaultPriority() {
            return _priorityRepository.Table.OrderBy(x => x.Value).FirstOrDefault() ?? CreateDefaultPriorities().First();
        }

        public IEnumerable<MessagePriority> CreateDefaultPriorities() {
            var priorities = new List<MessagePriority> {
                new MessagePriority {
                    Name = "Low",
                    DisplayText = "Low",
                    Value = 1
                },
                new MessagePriority {
                    Name = "Normal",
                    DisplayText = "Normal",
                    Value = 2
                },
                new MessagePriority {
                    Name = "High",
                    DisplayText = "High",
                    Value = 3
                },
            };

            foreach (var priority in priorities) {
                _priorityRepository.Create(priority);
            }

            return priorities;
        }

        public void DeletePriority(MessagePriority priority) {
            priority.Archived = true;
            priority.ArchivedUtc = _clock.UtcNow;
        }

        public IEnumerable<MessageQueue> GetIdleQueues() {
            return _queueRepository.Table.Where(x => x.Status == MessageQueueStatus.Idle).Select(x => ActivateQueue(x));
        }

        public void EnterProcessingStatus(MessageQueue queue) {
            if(queue == null) throw new ArgumentNullException("queue");
            if (queue.Status == MessageQueueStatus.Paused) throw new InvalidOperationException("Cannot process a paused queue. Think about it.");
            if (queue.Status == MessageQueueStatus.Processing) throw new InvalidOperationException("Cannot process an already processing queue. What's the point?");

            queue.Status = MessageQueueStatus.Processing;
            queue.StartedUtc = _clock.UtcNow;
        }

        public void ExitProcessingStatus(MessageQueue queue) {
            if(queue == null) throw new ArgumentNullException("queue");
            if(queue.Status == MessageQueueStatus.Paused) throw new InvalidOperationException("Cannot stop a paused queue.");
            if(queue.Status == MessageQueueStatus.Idle) throw new InvalidOperationException("Only processing queues can be stopped.");

            queue.Status = MessageQueueStatus.Idle;
            queue.EndedUtc = _clock.UtcNow;
        }

        public IEnumerable<MessageQueue> GetQueues() {
            return _queueRepository.Table.Select(ActivateQueue);
        }

        public int CountMessages(int queueId, QueuedMessageStatus? status = null) {
            return GetMessagesQuery(queueId, status).Count();
        }

        public IQueryable<QueuedMessage> GetMessages(int queueId, QueuedMessageStatus? status = null, int startIndex = 0, int pageSize = 10) {
            return GetMessagesQuery(queueId, status).Skip(startIndex).Take(pageSize).Select(ActivateMessage).AsQueryable();
        }

        public QueuedMessage GetMessage(int id) {
            return ActivateMessage(_messageRepository.Get(id));
        }

        public MessageQueue CreateQueue() {
            var record = new MessageQueueRecord {
                Status = MessageQueueStatus.Idle
            };
            _queueRepository.Create(record);
            return ActivateQueue(record);
        }

        MessageQueue IMessageQueueManager.CreateDefaultQueue() {
            var queue = CreateQueue();
            queue.Name = "Default";
            return queue;
        }

        public IQueryable<QueuedMessageRecord> GetMessagesQuery(int queueId, QueuedMessageStatus? status = null) {
            var query = _messageRepository.Table.Where(x => x.QueueId == queueId);

            if (status != null)
                query = query.Where(x => x.Status == status.Value);

            query = query.OrderByDescending(x => x.CreatedUtc);

            return query;
        }

        public IEnumerable<QueuedMessage> GetPendingMessages(int queueId, int pageSize = 10) {
            return _messageRepository.Table
                .Where(x => x.Status == QueuedMessageStatus.Pending && x.QueueId == queueId)
                .OrderBy(x => x.Priority.Value)
                .ThenBy(x => x.CreatedUtc)
                .Take(pageSize)
                .Select(ActivateMessage)
                .ToList();
        }

        private QueuedMessage ActivateMessage(QueuedMessageRecord record) {
            return new QueuedMessage(record) {
                QueueField = new Lazy<MessageQueue>(() => GetQueue(record.QueueId)), 
                RecipientsField = new Lazy<IEnumerable<MessageRecipient>>(() => ParseRecipients(record.Recipients)), 
                ChannelField = new Lazy<IMessageChannel>(() => GetChannel(record.ChannelName))
            };
        }

        private MessageQueue ActivateQueue(MessageQueueRecord record) {
            return new MessageQueue(record);
        }

        private static IEnumerable<MessageRecipient> ParseRecipients(string data) {
            return String.IsNullOrWhiteSpace(data) ? Enumerable.Empty<MessageRecipient>() : JsonConvert.DeserializeObject<List<MessageRecipient>>(data);
        }

        private MessageQueueRecord CreateDefaultQueue() {
            var queue = new MessageQueueRecord {
                Name = "Default",
                Status = MessageQueueStatus.Idle,
            };

            _queueRepository.Create(queue);
            return queue;
        }

        private static string ToJson(object value) {
            return value != null ? JsonConvert.SerializeObject(value) : null;
        }
    }
}