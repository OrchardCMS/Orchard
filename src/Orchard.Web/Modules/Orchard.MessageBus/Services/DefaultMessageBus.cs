using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Orchard.Logging;

namespace Orchard.MessageBus.Services {
    public class DefaultMessageBus : IMessageBus {
        private readonly IMessageBroker _messageBroker;

        public DefaultMessageBus(IEnumerable<IMessageBroker> messageBrokers) {
            _messageBroker = messageBrokers.FirstOrDefault();

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
 
        public void Subscribe(string channel, Action<string, string> handler) {
            if (_messageBroker == null) {
                return;
            }

            _messageBroker.Subscribe(channel, handler);
            Logger.Debug("{0} subscribed to {1}", GetHostName(), channel);
        }

        public void Publish(string channel, string message) {
            if (_messageBroker == null) {
                return;
            }

            _messageBroker.Publish(channel, message);
            Logger.Debug("{0} published {1}", GetHostName(), channel);
        }

        private string GetHostName() {
            // use the current host and the process id as two servers could run on the same machine
            return System.Net.Dns.GetHostName() + ":" + System.Diagnostics.Process.GetCurrentProcess().Id;
        }
    }
}