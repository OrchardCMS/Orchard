using System.Collections.Generic;
using Orchard.Logging;
using Orchard.Utility;

namespace Orchard.Extensions {
    public interface IHackInstallationGenerator  : IDependency {
        void GenerateInstallEvents();
    }

    public class HackInstallationGenerator : IHackInstallationGenerator {
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IExtensionManagerEvents> _extensionEvents;

        public HackInstallationGenerator(
            IExtensionManager extensionManager,
            IEnumerable<IExtensionManagerEvents> extensionEvents) {
            _extensionManager = extensionManager;
            _extensionEvents = extensionEvents;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void GenerateInstallEvents() {

            //TEMP: this is really part of the extension manager's job. an extension 
            // install event is being simulated here on each web app startup
            var enabled = new List<ExtensionEntry>();
            foreach (var extension in _extensionManager.ActiveExtensions()) {
                var context = new ExtensionEventContext {
                    Extension = extension,
                    EnabledExtensions = enabled.ToReadOnlyCollection(),
                };
                _extensionEvents.Invoke(x => x.Enabling(context), Logger);
                enabled.Add(extension);
                context.EnabledExtensions = enabled.ToReadOnlyCollection();
                _extensionEvents.Invoke(x => x.Enabled(context), Logger);
            }
        }
    }
}
