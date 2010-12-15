using System.Collections.Generic;
using Orchard.Messaging.Services;
using Orchard.Messaging.Models;

namespace Orchard.Tests.Messaging {
    public class MessagingChannelStub : IMessagingChannel {
        public List<MessageContext> Messages { get; private set; }

        public MessagingChannelStub() {
            Messages = new List<MessageContext>();
        }

        #region IMessagingChannel Members

        public void SendMessage(MessageContext message) {
            Messages.Add(message);
        }

        public IEnumerable<string> GetAvailableServices() {
            yield return "email";
        }

        #endregion
    }
}
