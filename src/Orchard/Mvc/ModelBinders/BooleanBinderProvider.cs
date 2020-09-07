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
            var requestBooleanValue = controllerContext.HttpContext.Request[bindingContext.ModelName].Split(',')[0]; //Html.CheckBox and Html.CheckBoxFor return "true,false" string
            if (!bool.TryParse(requestBooleanValue, out value)) {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, new FormatException());
            }
            return value;
        }
    }
}
