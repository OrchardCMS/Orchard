using Orchard.Messaging.Services;

namespace Orchard.Email.Services {
    public class DefaultEmailMessageChannelSelector : Component, IMessageChannelSelector {
        private readonly IWorkContextAccessor _workContextAccessor;
        public const string ChannelName = "Email";

        public DefaultEmailMessageChannelSelector(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public MessageChannelSelectorResult GetChannel(string messageType, object payload) {
            if (messageType == "Email") {
                var workContext = _workContextAccessor.GetContext();
                var channel = workContext.Resolve<ISmtpChannel>();
                return new MessageChannelSelectorResult {
                    Priority = 50,
                    MessageChannel = channel
                };
            }

            return null;
        }
    }
}
