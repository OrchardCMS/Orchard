using System.Collections.Generic;
using Orchard.Logging;

namespace Orchard.Messaging.Services {
    public class DefaultMessageService : Component, IMessageService {
        private readonly IMessageChannelManager _messageChannelManager;

        public DefaultMessageService(IMessageChannelManager messageChannelManager) {
            _messageChannelManager = messageChannelManager;
        }

        public void Send(string type, IDictionary<string, object> parameters) {
            var messageChannel = _messageChannelManager.GetMessageChannel(type, parameters);

            if (messageChannel == null) {
                Logger.Information("No channels where found to process a message of type {0}", type);
                return;
            }

            messageChannel.Process(parameters);
        }

    }
}