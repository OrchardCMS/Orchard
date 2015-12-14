using System.Collections.Concurrent;
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

        private static readonly object locker = new object();

        public static bool IsStarting { get; private set; }
        
        public readonly static string Channel = "ShellChanged";

        public DistributedShellStarter(IMessageBus messageBus, IWorkContextAccessor workContextAccessor) {
            _messageBus = messageBus;
            _workContextAccessor = workContextAccessor;
        }

        public void Activated() {
            _messageBus.Subscribe(Channel, (channel, message) => {
                // todo: this only handles changed tenants, we should consider handling started and stopped tenants

                using (var scope = _workContextAccessor.CreateWorkContextScope()) {
                    var shellSettings = scope.Resolve<ShellSettings>();
                    if (shellSettings != null) {

                        // todo: this doesn't work as the new tenants list is lost right after
                        var shellSettingsManagerEventHandler = scope.Resolve<IShellSettingsManagerEventHandler>();
                        var orchardHost = scope.Resolve<IOrchardHost>() as DefaultOrchardHost;

                        // We only want a single thread setting the "IsStarting" flag
                        lock (locker) { 
                            // Set a flag indicating that we are in the process of activating the shell. 
                            // This is used to prevent recursive message bus calls.
                            IsStarting = true;
                            shellSettingsManagerEventHandler.Saved(shellSettings);

                            if(orchardHost != null) {
                                var startUpdatedShellsMethod = typeof(DefaultOrchardHost).GetMethod("StartUpdatedShells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                startUpdatedShellsMethod.Invoke(orchardHost, null);
                            }

                            IsStarting = false;
                        }
                    }
                }
            });
        }

        public void Terminating() {
        }
    }
}