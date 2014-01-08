using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orchard.Environment;
using Orchard.Logging;
using Orchard.Messaging.Models;
using Orchard.Services;
using Orchard.TaskLease.Services;

namespace Orchard.Messaging.Services {
    public class MessageQueueProcessor : IMessageQueueProcessor {
        private readonly Work<IMessageQueueService> _messageQueueService;
        private readonly Work<IMessageService> _messageService;
        private readonly Work<IClock> _clock;
        private readonly Work<ITaskLeaseService> _taskLeaseService;
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();

        public MessageQueueProcessor(
            Work<IMessageQueueService> messageQueueService, 
            Work<IMessageService> messageService,
            Work<IClock> clock,
            Work<ITaskLeaseService> taskLeaseService) {
            _messageQueueService = messageQueueService;
            _messageService = messageService;
            _clock = clock;
            _taskLeaseService = taskLeaseService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public void ProcessQueue() {
            // prevent two threads on the same machine to process the message queue
            if (_rwl.TryEnterWriteLock(0)) {
                try {
                    _taskLeaseService.Value.Acquire("MessageQueueProcessor", _clock.Value.UtcNow.AddMinutes(5));
                    IEnumerable<QueuedMessageRecord> messages;

                    while ((messages = _messageQueueService.Value.GetMessages(QueuedMessageStatus.Pending, 0, 10).ToArray()).Any()) {
                        foreach (var message in messages.AsParallel()) {
                            ProcessMessage(message);
                        }
                    }
                }
                finally {
                    _rwl.ExitWriteLock();
                }
            }
        }

        private void ProcessMessage(QueuedMessageRecord message) {

            message.StartedUtc = _clock.Value.UtcNow;
            message.Status = QueuedMessageStatus.Sending;

            Logger.Debug("Sending message ID {0}.", message.Id);
            
            try {
                _messageService.Value.Send(message.Type, message.Payload);
                message.Status = QueuedMessageStatus.Sent;
                Logger.Debug("Sent message ID {0}.", message.Id);
            }
            catch (Exception e) {
                message.Status = QueuedMessageStatus.Faulted;
                message.Result = e.ToString();
                Logger.Error(e, "An unexpected error while sending message {0}. Error message: {1}.", message.Id, e);
            }
            finally {
                message.CompletedUtc = _clock.Value.UtcNow;
            }
        }
    }
}