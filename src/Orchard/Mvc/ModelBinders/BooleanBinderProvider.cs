using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace Orchard.Mvc.ModelBinders {
    public class BooleanBinderProvider : IModelBinderProvider, IModelBinder {

        public IEnumerable<ModelBinderDescriptor> GetModelBinders() {
            return new[] {
                             new ModelBinderDescriptor {
                                                           ModelBinder = this,
                                                           Type = typeof(bool)
                                                       }
                         };
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            // returning null from here allows the downstream method to set its own default
            var value = false;
            if (bindingContext != null) {
                if (bindingContext.ValueProvider
                    ?.GetValue(bindingContext.ModelName) == null) {
                    // this is the case where we are not receiving a possible value for the boolean.
                    // Returning null is ok here, and will let the downstream method set its own defaults.
                    return null;
                }
            }
            try {
                var attemptedValues = bindingContext.ValueProvider
                    .GetValue(bindingContext.ModelName)
                    .AttemptedValue
                    // for bool, AttemptedValue may be "true,false" because in the form there may be
                    // a checkbox and an hidden input.
                    .Split(new char[] { ',' });
                value = attemptedValues
                    .Select(v => Convert.ToBoolean(v))
                    // This Aggregate operation works because the "true" is normally only there when
                    // the checkbox was selected, while the false is always there thanks to the hidden
                    // input.
                    .Aggregate((a, b) => a || b);
                // The steps above don't affect binding booleans from any where other than a form,
                // because those won't give us here a list of possible values to aggregate.
            } catch {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, new FormatException());
                return null;
            }
            return value;
        }
    }
}
