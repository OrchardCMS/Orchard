using System.Collections.Generic;
using Orchard.Messaging.Services;
using Orchard.Messaging.Models;

namespace Orchard.Tests.Messaging {
    public class MessagingChannelStub : IMessageChannel {
        public List<IDictionary<string, object>> Messages { get; private set; }
        
        public MessagingChannelStub() {
            Messages = new List<IDictionary<string, object>>();
        }

        public void Process(IDictionary<string, object> parameters) {
            Messages.Add(parameters);
        }
    }
}
