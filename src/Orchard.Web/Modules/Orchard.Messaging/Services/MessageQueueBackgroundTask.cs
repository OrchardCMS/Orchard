using Orchard.Environment.Extensions;
using Orchard.Tasks;

namespace Orchard.Messaging.Services {
    [OrchardFeature("Orchard.Messaging.Queuing")]
    public class MessageQueueBackgroundTask : Component, IBackgroundTask {
        private readonly IMessageQueueProcessor _messageQueueProcessor;
        public MessageQueueBackgroundTask(IMessageQueueProcessor messageQueueProcessor) {
            _messageQueueProcessor = messageQueueProcessor;
        }

        public void Sweep() {
            _messageQueueProcessor.ProcessQueues();
        }
    }
}