using System.Linq;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;

namespace Orchard.MessageBus.Services {

    public interface IDistributedShellStarter : ISingletonDependency {

    }

    [OrchardFeature("Orchard.MessageBus.DistributedShellRestart")]
    public class DistributedShellStarter : IDistributedShellStarter, IOrchardShellEvents {
        private readonly IWorkContextAccessor _workContextAccessor;

        private readonly IMessageBus _messageBus;

        public readonly static string Channel = "ShellChanged";

        public DistributedShellStarter(IMessageBus messageBus, IWorkContextAccessor workContextAccessor) {
            _messageBus = messageBus;
            _workContextAccessor = workContextAccessor;
        }

        public void Activated() {
            _messageBus.Subscribe(Channel, (channel, tenantName) => {
                // todo: this only handles changed tenants, we should consider handling started and stopped tenants

                using (var scope = _workContextAccessor.CreateWorkContextScope()) {
                    var shellSettings = scope.Resolve<ShellSettings>();
                    if (shellSettings != null && shellSettings.Name == tenantName) {

                        // todo: this doesn't work as the new tenants list is lost right after
                        var shellSettingsManagerEventHandler = scope.Resolve<IShellSettingsManagerEventHandler>();
                        shellSettingsManagerEventHandler.Saved(shellSettings);

                        var orchardHost = scope.Resolve<IOrchardHost>() as DefaultOrchardHost;
                        if(orchardHost != null) {
                            var startUpdatedShellsMethod = typeof(DefaultOrchardHost).GetMethod("StartUpdatedShells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            startUpdatedShellsMethod.Invoke(orchardHost, null);
                        }
                    }
                }
            });
        }

        public void Terminating() {
        }
    }
}