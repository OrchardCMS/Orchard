using System;
using System.Globalization;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Fields.Fields;
using Orchard.Layouts.Services;

namespace Orchard.DynamicForms.Bindings {
    public class NumericFieldBindings : Component, IBindingProvider {
        private readonly ICultureAccessor _cultureAccessor;

        public NumericFieldBindings(ICultureAccessor cultureAccessor) {
            _cultureAccessor = cultureAccessor;
        }

        public void Describe(BindingDescribeContext context) {
            context.For<NumericField>()
                .Binding("Value", (contentItem, field, s) => field.Value = ToDecimal(s));
        }

        private Decimal? ToDecimal(string s) {
            Decimal value;
            return Decimal.TryParse(s, NumberStyles.Any, _cultureAccessor.CurrentCulture, out value) ? value : (Decimal?)null;
        }
    }
}