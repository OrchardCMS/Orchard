using System;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Fields.Fields;

namespace Orchard.DynamicForms.Bindings
{
    public class BooleanFieldBindings : Component, IBindingProvider
    {

        public void Describe(BindingDescribeContext context)
        {
            context.For<BooleanField>()
                .Binding("Value", (contentItem, field, s) => field.Value = IsTrueish(s));
        }

        private bool IsTrueish(string s)
        {
            return !string.IsNullOrWhiteSpace(s) && !string.Equals("false", s, StringComparison.OrdinalIgnoreCase) && !string.Equals(T("No").Text, s, StringComparison.OrdinalIgnoreCase);
        }
    }
}