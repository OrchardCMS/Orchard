using Orchard.ContentManagement;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Fields.Fields;

namespace Orchard.DynamicForms.Bindings {
    public class NumericFieldBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<NumericField>()
                .Binding("Value", (contentItem, field, s) => field.Value = XmlHelper.Parse<decimal>(s));
        }
    }
}