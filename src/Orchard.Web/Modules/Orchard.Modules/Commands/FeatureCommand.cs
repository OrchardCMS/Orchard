using System.Linq;
using Orchard.Commands;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Commands {
    public class FeatureCommand : DefaultOrchardCommandHandler {
        private readonly IModuleService _moduleService;

        public FeatureCommand(IModuleService moduleService) {
            _moduleService = moduleService;
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

                var categories = _moduleService.GetAvailableFeatures().GroupBy(f => f.Descriptor.Category);
                foreach (var category in categories) {
                    Context.Output.WriteLine(T("{0}", category.Key.OrDefault("General")));
                    foreach (var feature in category.OrderBy(f => f.Descriptor.Name)) {
                        Context.Output.WriteLine(T("  {0}", feature.Descriptor.Name));
                        Context.Output.WriteLine(T("    State:         {0}", feature.IsEnabled ? T("Enabled") : T("Disabled")));
                        Context.Output.WriteLine(T("    Description:   {0}", feature.Descriptor.Description.OrDefault("<none>")));
                        Context.Output.WriteLine(T("    Category:      {0}", feature.Descriptor.Category.OrDefault("<none>")));
                        Context.Output.WriteLine(T("    Module:        {0}", feature.Descriptor.Extension.Name.OrDefault("<none>")));
                        Context.Output.WriteLine(T("    Dependencies:  {0}", string.Join(",", feature.Descriptor.Dependencies).OrDefault("<none>")));
                    }
                }
            }
        }

        [CommandHelp("feature enable <feature-name-1> ... <feature-name-n>\r\n\t" + "Enable one or more features")]
        [CommandName("feature enable")]
        public void Enable(params string[] featureNames) {
            Context.Output.WriteLine(T("Enabling features {0}", string.Join(",", featureNames)));
            _moduleService.EnableFeatures(featureNames);
            Context.Output.WriteLine(T("Enabled features  {0}", string.Join(",", featureNames)));
        }

        [CommandHelp("feature disable <feature-name-1> ... <feature-name-n>\r\n\t" + "Disable one or more features")]
        [CommandName("feature disable")]
        public void Disable(params string[] featureNames) {
            Context.Output.WriteLine(T("Disabling features {0}", string.Join(",", featureNames)));
            _moduleService.DisableFeatures(featureNames);
            Context.Output.WriteLine(T("Disabled features  {0}", string.Join(",", featureNames)));
        }
    }
}
