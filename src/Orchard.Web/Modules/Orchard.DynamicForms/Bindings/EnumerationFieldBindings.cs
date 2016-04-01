using System;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Fields.Fields;
using System.Linq;

namespace Orchard.DynamicForms.Bindings {
    public class EnumerationFieldBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<EnumerationField>()
                .Binding("SelectedValues",
                    (contentItem, field) => {
                        if (field.SelectedValues == null || field.SelectedValues.Length == 0)
                            return field.Value;
                        return field.SelectedValues.Aggregate((current, next) => current + "," + next);                        
                    }
                    , (contentItem, field, s) => {
                        if (String.IsNullOrWhiteSpace(s)) {
                            field.Value = "";
                            return;
                        }

                        var separators = new[] {',', ';'};
                        var hasMultipleValues = s.IndexOfAny(separators) >= 0;

                        if (hasMultipleValues)
                            field.SelectedValues = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        else {
                            field.Value = s;
                        }
                    });
        }
    }
}