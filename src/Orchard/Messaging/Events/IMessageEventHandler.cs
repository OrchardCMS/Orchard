using Orchard.Events;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Events {
    public interface IMessageEventHandler : IEventHandler {
        void Sending(MessageContext context);
        void Sent(MessageContext context);
    }
}
