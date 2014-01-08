using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Messaging.Models;
using Orchard.Services;
using Orchard.Settings;

namespace Orchard.Messaging.Services {
    public class MessageQueueService : IMessageQueueService {
        private readonly IClock _clock;
        private readonly IRepository<QueuedMessageRecord> _messageRepository;
        private readonly MessageSettingsPart _messageSettingsPart;

        public MessageQueueService(
            IClock clock, 
            IRepository<QueuedMessageRecord> messageRepository, 
            ISiteService siteService) {
            _clock = clock;
            _messageRepository = messageRepository;
            _messageSettingsPart = siteService.GetSiteSettings().As<MessageSettingsPart>();
        }

        public QueuedMessageRecord Enqueue(string channelName, object payload, int priority) {

            var queuedMessage = new QueuedMessageRecord {
                Payload = ToJson(payload),
                Type = channelName,
                CreatedUtc = _clock.UtcNow,
                Status = QueuedMessageStatus.Pending
            };

            _messageRepository.Create(queuedMessage);

            return queuedMessage;
        }

        public void Resume() {
            _messageSettingsPart.Status = MessageQueueStatus.Idle;
        }

        public void Pause() {
            _messageSettingsPart.Status = MessageQueueStatus.Paused;
        }

        public int GetMessagesCount(QueuedMessageStatus? status = null) {
            return GetMessagesQuery(status).Count();
        }

        public IEnumerable<QueuedMessageRecord> GetMessages(QueuedMessageStatus? status, int startIndex, int pageSize) {
            return GetMessagesQuery(status)
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();
        }

        public QueuedMessageRecord GetMessage(int id) {
            return _messageRepository.Get(id);
        }

        private IQueryable<QueuedMessageRecord> GetMessagesQuery(QueuedMessageStatus? status) {
            var query = _messageRepository.Table;

            if (status != null) {
                query = query.Where(x => x.Status == status.Value);
            }

            query = query
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.CreatedUtc);

            return query;
        }
        
        private static string ToJson(object value) {
            return value != null ? JsonConvert.SerializeObject(value) : null;
        }
    }
}