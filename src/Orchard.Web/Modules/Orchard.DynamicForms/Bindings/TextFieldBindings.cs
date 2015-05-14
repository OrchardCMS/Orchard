using Orchard.Core.Common.Fields;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Bindings {
    public class TextFieldBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<TextField>()
                .Binding("Text", (contentItem, field, s) => field.Value = s);
        }
    }
}