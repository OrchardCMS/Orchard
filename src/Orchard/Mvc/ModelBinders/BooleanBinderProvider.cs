using System;
using System.Collections.Generic;
using System.Web.Mvc;

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
            var value = false;
            try {
                value = Convert.ToBoolean(bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue);
            } catch {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, new FormatException());
            }
            return value;
        }
    }
}
