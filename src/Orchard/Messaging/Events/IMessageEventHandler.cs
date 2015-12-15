using System;
using Orchard.Events;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Events {
    [Obsolete]
    public interface IMessageEventHandler : IEventHandler {
        void Sending(MessageContext context);
        void Sent(MessageContext context);
    }
}
