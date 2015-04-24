using System;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;

namespace Orchard.DynamicForms.Handlers {
    public class ReadFormValuesHandler : FormElementEventHandlerBase {
        public override void GetElementValue(FormElement element, ReadElementValuesContext context) {

            if (String.IsNullOrWhiteSpace(element.Name))
                return;

            var key = element.Name;
            var valueProviderResult = context.ValueProvider.GetValue(key);

            if (String.IsNullOrWhiteSpace(key) || valueProviderResult == null)
                return;

            var items = valueProviderResult.RawValue as string[];
            if (items == null)
                return;

            var combinedValues = String.Join(",", items);
            context.Output[key] = combinedValues;
            element.RuntimeValue = combinedValues;
            element.PostedValue = combinedValues;
        }
    }
}