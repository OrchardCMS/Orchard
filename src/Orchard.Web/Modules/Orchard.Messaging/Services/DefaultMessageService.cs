using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;

namespace Orchard.Messaging.Services {
    public class DefaultMessageService : Component, IMessageService {
        private readonly IEnumerable<IMessageChannelSelector> _messageChannelSelectors;

        public DefaultMessageService(IEnumerable<IMessageChannelSelector> messageChannelSelectors) {
            _messageChannelSelectors = messageChannelSelectors;
        }

        public void Send(string type, string payload) {
            var messageChannelResult = _messageChannelSelectors
                .Select(x => x.GetChannel(type, payload))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault();

            if (messageChannelResult == null || messageChannelResult.MessageChannel == null) {
                Logger.Information("No channels where found to process a message of type {0}", type);
                return;
            }

            messageChannelResult.MessageChannel.Process(payload);
        }
    }
}