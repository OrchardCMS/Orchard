using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace Orchard.MessageBus.Services {
    public class DefaultMessageBus : IMessageBus {
        private readonly IMessageBroker _messageBroker;

        public DefaultMessageBus(IEnumerable<IMessageBroker> messageBrokers) {
            _messageBroker = messageBrokers.FirstOrDefault();
        }

        public void Subscribe(string channel, Action<string, string> handler) {
            if (_messageBroker == null) {
                return;
            }

            _messageBroker.Subscribe(channel, handler);
        }

        public void Publish(string channel, string message) {
            if (_messageBroker == null) {
                return;
            }

            _messageBroker.Publish(channel, message);
        }
    }
}