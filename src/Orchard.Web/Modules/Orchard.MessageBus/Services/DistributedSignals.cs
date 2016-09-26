using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.MessageBus.Services;
using Orchard.Services;

namespace Orchard.MessageBus.Services {
    [OrchardFeature("Orchard.MessageBus.DistributedSignals")]
    [OrchardSuppressDependency("Orchard.Caching.Signals")]
    public class DistributedSignals : Signals, ISignals, IOrchardShellEvents {
        private readonly IMessageBus _messageBus;

        public DistributedSignals(IMessageBus messageBus) {
            _messageBus = messageBus;
        }

        void ISignals.Trigger<T>(T signal) {
            base.Trigger(signal);
            _messageBus.Publish("Signal", signal.ToString());
        }

        IVolatileToken ISignals.When<T>(T signal) {
            return base.When(signal);
        }

        public void Activated() {
            _messageBus.Subscribe("Signal", (channel, message) => {
                base.Trigger(message);
            });
        }

        public void Terminating() {
        }
    }
}