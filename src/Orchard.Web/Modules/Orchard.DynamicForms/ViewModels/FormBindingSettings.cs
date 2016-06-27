using System;
using System.Collections.Generic;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.ViewModels {
    public class FormBindingSettings {
        public FormBindingSettings() {
            Parts = new List<PartBindingSettings>();
        }

        public IEnumerable<ContentPartBindingDescriptor> AvailableBindings { get; set; }
        public IList<PartBindingSettings> Parts { get; set; }

        public void Store(IDictionary<string, string> data) {
            for (var partIndex = 0; partIndex < Parts.Count; partIndex++) {
                var part = Parts[partIndex];

                data[String.Format("FormBindingSettings.Parts[{0}].Name", partIndex)] = part.Name;

                for (var partBindingIndex = 0; partBindingIndex < part.Bindings.Count; partBindingIndex++) {
                    var partBinding = part.Bindings[partBindingIndex];
                    data[String.Format("FormBindingSettings.Parts[{0}].Bindings[{1}].Name", partIndex, partBindingIndex)] = partBinding.Name;
                    data[String.Format("FormBindingSettings.Parts[{0}].Bindings[{1}].Enabled", partIndex, partBindingIndex)] = partBinding.Enabled.ToString();
                }

                for (var fieldIndex = 0; fieldIndex < part.Fields.Count; fieldIndex++) {
                    var field = part.Fields[fieldIndex];
                    data[String.Format("FormBindingSettings.Parts[{0}].Fields[{1}].Name", partIndex, fieldIndex)] = field.Name;

                    for (var fieldBindingIndex = 0; fieldBindingIndex < field.Bindings.Count; fieldBindingIndex++) {
                        var fieldBinding = field.Bindings[fieldBindingIndex];

                        data[String.Format("FormBindingSettings.Parts[{0}].Fields[{1}].Bindings[{2}].Name", partIndex, fieldIndex, fieldBindingIndex)] = fieldBinding.Name;
                        data[String.Format("FormBindingSettings.Parts[{0}].Fields[{1}].Bindings[{2}].Enabled", partIndex, fieldIndex, fieldBindingIndex)] = fieldBinding.Enabled.ToString();
                    }
                    
                }
            }
        }
    }
}