using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;

namespace Orchard.MessageBus.Services {
    [OrchardFeature("Orchard.MessageBus.DistributedShellRestart")]
    public class DistributedShellTrigger : IShellDescriptorManagerEventHandler, IShellSettingsManagerEventHandler {
    
        private readonly IMessageBus _messageBus;
        private readonly IDistributedShellStarter _shellStarter;
        
        public DistributedShellTrigger(IShellSettingsManager shellSettingsManager, IMessageBus messageBus, IShellSettingsManagerEventHandler shellSettingsManagerEventHandler, IDistributedShellStarter shellStarter) {
            _messageBus = messageBus;
            _shellStarter = shellStarter;
        }

        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {

            // If this method was called as a result of a published message to 
            // start the shell, then prevent a recursive loop.
            if(!_shellStarter.IsStarting()) {
                _messageBus.Publish(DistributedShellStarter.Channel, tenant);
            }
        }

        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings) {

            // If this method was called as a result of a published message to 
            // start the shell, then prevent a recursive loop.
            if (!_shellStarter.IsStarting()) {
                _messageBus.Publish(DistributedShellStarter.Channel, settings.Name);
            }
        }
    }
}