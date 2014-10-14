using System.Collections.Generic;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.ViewModels {
    public class FormBindingSettings {
        public FormBindingSettings() {
            Parts = new List<PartBindingSettings>();
        }

        public IEnumerable<ContentPartBindingDescriptor> AvailableBindings { get; set; }
        public IList<PartBindingSettings> Parts { get; set; }
    }
}