using System;

namespace Orchard.Messaging.Services {
    public interface IMessageChannelSelector : IDependency {
        MessageChannelSelectorResult GetChannel(string messageType, object payload);
    }

    public class MessageChannelSelectorResult {
        public int Priority { get; set; }
        public Func<IMessageChannel> MessageChannel { get; set; }
    }
}