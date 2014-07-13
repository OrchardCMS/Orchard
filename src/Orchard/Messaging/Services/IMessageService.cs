using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Messaging.Services {
    public interface IMessageService : IEventHandler {
        void Send(string type, IDictionary<string, object> parameters);
    }
}