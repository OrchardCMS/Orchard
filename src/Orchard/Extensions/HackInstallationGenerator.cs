using System;
using System.Collections.Generic;
using Orchard.Logging;
using Orchard.Utility;

namespace Orchard.Extensions {
    public interface IHackInstallationGenerator  : IDependency {
        void GenerateInstallEvents();
        void GenerateActivateEvents();
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

        public void GenerateActivateEvents() {
            //TEMP: This is really part of the extension manager's job.
            var extensions = _extensionManager.ActiveExtensions().ToReadOnlyCollection();
            foreach (var extension in extensions) {
                var context = new ExtensionEventContext {
                    Extension = extension,
                    EnabledExtensions = extensions,
                };
                _extensionEvents.Invoke(x => x.Activating(context), Logger);
                _extensionEvents.Invoke(x => x.Activated(context), Logger);
            }
        }
    }
}
