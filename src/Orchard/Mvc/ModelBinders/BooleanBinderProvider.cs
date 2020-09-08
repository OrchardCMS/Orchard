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
                value = (bool)bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ConvertTo(typeof(bool));
            } catch {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, new FormatException());
            }
            return value;
        }
    }
}
