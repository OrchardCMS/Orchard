using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Features;
using Orchard.Modules.Services;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Commands {
    public class FeatureCommands : DefaultOrchardCommandHandler {
        private readonly IModuleService _moduleService;
        private readonly INotifier _notifier;
        private readonly IFeatureManager _featureManager;
        private readonly ShellDescriptor _shellDescriptor;

        public FeatureCommands(IModuleService moduleService, INotifier notifier, IFeatureManager featureManager, ShellDescriptor shellDescriptor) {
            _moduleService = moduleService;
            _notifier = notifier;
            _featureManager = featureManager;
            _shellDescriptor = shellDescriptor;
        }

        [OrchardSwitch]
        public bool Summary { get; set; }

        [CommandHelp("feature list [/Summary:true|false]\r\n\t" + "Display list of available features")]
        [CommandName("feature list")]
        [OrchardSwitches("Summary")]
        public void List() {
            var enabled = _shellDescriptor.Features.Select(x => x.Name);
            if (Summary) {
                foreach (var feature in _featureManager.GetAvailableFeatures().OrderBy(f => f.Id)) {
                    Context.Output.WriteLine(T("{0}, {1}", feature.Id, enabled.Contains(feature.Id) ? T("Enabled") : T("Disabled")));
                }
            }
            else {
                Context.Output.WriteLine(T("List of available features"));
                Context.Output.WriteLine(T("--------------------------"));

                var categories = _featureManager.GetAvailableFeatures().ToList().GroupBy(f => f.Category);
                foreach (var category in categories) {
                    Context.Output.WriteLine(T("Category: {0}", category.Key.OrDefault(T("General"))));
                    foreach (var feature in category.OrderBy(f => f.Id)) {
                        Context.Output.WriteLine(T("  Name: {0}", feature.Id));
                        Context.Output.WriteLine(T("    State:         {0}", enabled.Contains(feature.Id) ? T("Enabled") : T("Disabled")));
                        Context.Output.WriteLine(T("    Description:   {0}", feature.Description.OrDefault(T("<none>"))));
                        Context.Output.WriteLine(T("    Category:      {0}", feature.Category.OrDefault(T("<none>"))));
                        Context.Output.WriteLine(T("    Module:        {0}", feature.Extension.Id.OrDefault(T("<none>"))));
                        Context.Output.WriteLine(T("    Dependencies:  {0}", string.Join(", ", feature.Dependencies).OrDefault(T("<none>"))));
                    }
                }
            }
        }

        [CommandHelp("feature enable <feature-name-1> ... <feature-name-n>\r\n\t" + "Enable one or more features")]
        [CommandName("feature enable")]
        public void Enable(params string[] featureNames) {
            Context.Output.WriteLine(T("Enabling features {0}", string.Join(",", featureNames)));
            bool listAvailableFeatures = false;
            List<string> featuresToEnable = new List<string>();
            string[] availableFeatures = _featureManager.GetAvailableFeatures().Select(x => x.Id).ToArray();
            foreach (var featureName in featureNames) {
                if (availableFeatures.Contains(featureName)) {
                    featuresToEnable.Add(featureName);
                }
                else {
                    Context.Output.WriteLine(T("Could not find feature {0}", featureName));
                    listAvailableFeatures = true;
                }
            }
            if (featuresToEnable.Count != 0) {
                _moduleService.EnableFeatures(featuresToEnable, true);
                foreach (var entry in _notifier.List()) {
                    Context.Output.WriteLine(entry.Message);
                }
            }
            else {
                Context.Output.WriteLine(T("Could not enable features: {0}", string.Join(",", featureNames)));
                listAvailableFeatures = true;
            }
            if (listAvailableFeatures)
                Context.Output.WriteLine(T("Available features are : {0}", string.Join(",", availableFeatures)));
        }

        [CommandHelp("feature disable <feature-name-1> ... <feature-name-n>\r\n\t" + "Disable one or more features")]
        [CommandName("feature disable")]
        public void Disable(params string[] featureNames) {
            Context.Output.WriteLine(T("Disabling features {0}", string.Join(",", featureNames)));
            _moduleService.DisableFeatures(featureNames, true);
            Context.Output.WriteLine(T("Disabled features  {0}", string.Join(",", featureNames)));
        }
    }
}
