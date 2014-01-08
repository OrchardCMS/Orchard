namespace Orchard.Messaging.Services {
    /// <summary>
    /// Default empty implementation of <see cref="IMessageChannelSelector"/>
    /// </summary>
    public class NullMessageChannelSelector : IMessageChannelSelector {
        public MessageChannelSelectorResult GetChannel(string messageType, object payload) {
            return null;
        }
    }
}