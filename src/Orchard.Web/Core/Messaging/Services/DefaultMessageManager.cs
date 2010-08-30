using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Messaging.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Settings;

namespace Orchard.Core.Messaging.Services {
    public class DefaultMessageManager : IMessageManager {
        private readonly IMessageEventHandler _messageEventHandler;
        private readonly IEnumerable<IMessagingChannel> _channels;
        private readonly IRepository<Message> _messageRepository;
        
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        public ILogger Logger { get; set; }

        public DefaultMessageManager(
            IMessageEventHandler messageEventHandler,
            IEnumerable<IMessagingChannel> channels,
            IRepository<Message> messageRepository) {
            _messageEventHandler = messageEventHandler;
            _channels = channels;
            _messageRepository = messageRepository;
        }

        public void Send(Message message) {
            if ( !HasChannels() )
                return;

            var messageSettings = CurrentSite.As<MessageSettingsPart>().Record;

            if ( messageSettings == null || String.IsNullOrWhiteSpace(messageSettings.DefaultChannelService) ) {
                return;
            }

            Logger.Information("Sending message {0}", message.Type);
            try {

                // if the service is not explicit, use the default one, as per settings configuration
                if ( String.IsNullOrWhiteSpace(message.Service) ) {
                    message.Service = messageSettings.DefaultChannelService;
                }

                var context = new MessageContext(message);

                _messageEventHandler.Sending(context);

                foreach ( var channel in _channels ) {
                    channel.SendMessage(context);
                }

                _messageEventHandler.Sent(context);

                Logger.Information("Message {0} sent", message.Type);
            }
            catch ( Exception e ) {
                Logger.Error(e, "An error occured while sending the message {0}", message.Type);
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
