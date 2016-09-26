using System;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Fields.Fields;

namespace Orchard.DynamicForms.Bindings {
    public class EnumerationFieldBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<EnumerationField>()
                .Binding("SelectedValues", (contentItem, field, s) => {
                    var items = (s ?? "").Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                    field.SelectedValues = items;
                });
        }
    }
}