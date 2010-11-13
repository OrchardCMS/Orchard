using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Messaging.Models;
using Orchard.Logging;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Messaging.Services {
    public class DefaultMessageManager : IMessageManager {
        private readonly IMessageEventHandler _messageEventHandler;
        private readonly IEnumerable<IMessagingChannel> _channels;
        private readonly IOrchardServices _orchardServices;

        public ILogger Logger { get; set; }

        public DefaultMessageManager(
            IMessageEventHandler messageEventHandler,
            IEnumerable<IMessagingChannel> channels,
            IOrchardServices orchardServices) {
            _messageEventHandler = messageEventHandler;
            _channels = channels;
            _orchardServices = orchardServices;
        }

        public void Send(ContentItemRecord recipient, string type, string service = null, Dictionary<string, string> properties = null) {
            if ( !HasChannels() )
                return;

            var messageSettings = _orchardServices.WorkContext.CurrentSite.As<MessageSettingsPart>().Record;

            if ( messageSettings == null || String.IsNullOrWhiteSpace(messageSettings.DefaultChannelService) ) {
                return;
            }

            Logger.Information("Sending message {0}", type);
            try {

                // if the service is not explicit, use the default one, as per settings configuration
                if (String.IsNullOrWhiteSpace(service)) {
                    service = messageSettings.DefaultChannelService;
                }

                var context = new MessageContext {
                    Recipient = recipient,
                    Type = type,
                    Service = service
                };

                try {

                    if (properties != null) {
                        foreach (var key in properties.Keys)
                            context.Properties.Add(key, properties[key]);
                    }

                    _messageEventHandler.Sending(context);

                    foreach (var channel in _channels) {
                        channel.SendMessage(context);
                    }

                    _messageEventHandler.Sent(context);
                }
                finally {
                    context.MailMessage.Dispose();
                }

                Logger.Information("Message {0} sent", type);
            }
            catch ( Exception e ) {
                Logger.Error(e, "An error occured while sending the message {0}", type);
            }
        }

        public bool HasChannels() {
            return _channels.Any();
        }

        public IEnumerable<string> GetAvailableChannelServices() {
            return _channels.SelectMany(c => c.GetAvailableServices());
        }
    }
}
