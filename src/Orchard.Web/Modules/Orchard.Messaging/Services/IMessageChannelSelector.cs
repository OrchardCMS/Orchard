namespace Orchard.Messaging.Services {
    public interface IMessageChannelSelector : IDependency {
        MessageChannelSelectorResult GetChannel(string messageType, object payload);
    }

    public class MessageChannelSelectorResult {
        public int Priority { get; set; }
        public IMessageChannel MessageChannel { get; set; }
    }
}