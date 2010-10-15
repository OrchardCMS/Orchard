using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Commands {
    public class FeatureCommands : DefaultOrchardCommandHandler {
        private readonly IModuleService _moduleService;
        private readonly INotifier _notifier;

        public FeatureCommands(IModuleService moduleService, INotifier notifier) {
            _moduleService = moduleService;
            _notifier = notifier;
        }

        [OrchardSwitch]
        public bool Summary { get; set; }

        [CommandHelp("feature list [/Summary:true|false]\r\n\t" + "Display list of available features")]
        [CommandName("feature list")]
        [OrchardSwitches("Summary")]
        public void List() {
            if (Summary) {
                foreach (var feature in _moduleService.GetAvailableFeatures().OrderBy(f => f.Descriptor.Name)) {
                    Context.Output.WriteLine(T("{0}, {1}", feature.Descriptor.Name, feature.IsEnabled ? T("Enabled") : T("Disabled")));
                }
            }
            else {
                Context.Output.WriteLine(T("List of available features"));
                Context.Output.WriteLine(T("--------------------------"));

                var categories = _moduleService.GetAvailableFeatures().ToList().GroupBy(f => f.Descriptor.Category);
                foreach (var category in categories) {
                    Context.Output.WriteLine(T("Category: {0}", category.Key.OrDefault(T("General"))));
                    foreach (var feature in category.OrderBy(f => f.Descriptor.Name)) {
                        Context.Output.WriteLine(T("  Name: {0}", feature.Descriptor.Name));
                        Context.Output.WriteLine(T("    State:         {0}", feature.IsEnabled ? T("Enabled") : T("Disabled")));
                        Context.Output.WriteLine(T("    Description:   {0}", feature.Descriptor.Description.OrDefault(T("<none>"))));
                        Context.Output.WriteLine(T("    Category:      {0}", feature.Descriptor.Category.OrDefault(T("<none>"))));
                        Context.Output.WriteLine(T("    Module:        {0}", feature.Descriptor.Extension.Name.OrDefault(T("<none>"))));
                        Context.Output.WriteLine(T("    Dependencies:  {0}", string.Join(", ", feature.Descriptor.Dependencies).OrDefault(T("<none>"))));
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
            string[] availableFeatures = _moduleService.GetAvailableFeatures().Select(x => x.Descriptor.Name).ToArray();
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
