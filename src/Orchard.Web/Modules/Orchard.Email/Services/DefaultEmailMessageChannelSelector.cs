using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.Messaging.Services;

namespace Orchard.Email.Services {
    public class DefaultEmailMessageChannelSelector : Component, IMessageChannelSelector {
        private readonly IOrchardServices _services;
        public const string ChannelName = "Email";

        public DefaultEmailMessageChannelSelector(IOrchardServices services) {
            _services = services;
        }
        
        public MessageChannelSelectorResult GetChannel(string messageType, object payload) {
            if (messageType == "Email") {
                return new MessageChannelSelectorResult {
                    Priority = 50,
                    MessageChannel = new SmtpMessageChannel(_services.WorkContext.CurrentSite.As<SmtpSettingsPart>())
                };
            }

            return null;
        }
    }
}
