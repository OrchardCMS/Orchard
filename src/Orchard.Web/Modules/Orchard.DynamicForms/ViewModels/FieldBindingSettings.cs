using System.Collections.Generic;

namespace Orchard.DynamicForms.ViewModels {
    public class FieldBindingSettings {
        public FieldBindingSettings() {
            Bindings = new List<BindingSettings>();
        }

        public string Name { get; set; }
        public IList<BindingSettings> Bindings { get; set; }
    }
}