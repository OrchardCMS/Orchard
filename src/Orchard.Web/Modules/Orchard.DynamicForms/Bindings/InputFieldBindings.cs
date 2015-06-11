using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Fields.Fields;

namespace Orchard.DynamicForms.Bindings {
    public class InputFieldBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<InputField>()
                .Binding("Text", (contentItem, field, s) => field.Value = s);
        }
    }
}