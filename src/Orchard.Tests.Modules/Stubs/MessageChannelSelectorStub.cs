using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Messaging.Services;

namespace Orchard.Tests.Modules.Stubs {

    public class MessageChannelSelectorStub : IMessageChannelSelector {

        private IMessageChannel _messageChannel;
        public MessageChannelSelectorStub(IMessageChannel messageChannel) {
            _messageChannel = messageChannel;
        }

        public MessageChannelSelectorResult GetChannel(string messageType, object payload) {
            return new MessageChannelSelectorResult {
                Priority = 0,
                MessageChannel = () => { return _messageChannel; }
            };
        }
    }
}
