using Orchard.Tasks;

namespace Orchard.Messaging.Services {
    public class MessageQueueBackgroundTask : Component, IBackgroundTask {
        private readonly IMessageQueueProcessor _messageQueueProcessor;
        public MessageQueueBackgroundTask(IMessageQueueProcessor messageQueueProcessor) {
            _messageQueueProcessor = messageQueueProcessor;
        }

        public void Sweep() {
            _messageQueueProcessor.ProcessQueue();
        }
    }
}