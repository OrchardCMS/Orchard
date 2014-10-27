using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;

namespace Orchard.DynamicForms.Helpers {
    internal static class FormControllerExtensions {
        internal static void TransferFormSubmission(this Controller controller, Form form, NameValueCollection values) {
            controller.TempData[String.Format("Form_ModelState_{0}", form.Name)] = controller.ModelState;
            controller.TempData[String.Format("Form_Values_{0}", form.Name)] = values;
        }

        internal static ModelStateDictionary FetchModelState(this Controller controller, Form form) {
            return (ModelStateDictionary)controller.TempData[String.Format("Form_ModelState_{0}", form.Name)];
        }

        internal static NameValueCollection FetchPostedValues(this Controller controller, Form form) {
            return (NameValueCollection)controller.TempData[String.Format("Form_Values_{0}", form.Name)] ?? new NameValueCollection();
        }

        internal static void ApplyAnyModelErrors(this Controller controller, Form form, ModelStateDictionary modelState) {
            var hasErrors = modelState != null && !modelState.IsValid;

            if (hasErrors) {
                foreach (var state in modelState) {
                    if (state.Value.Errors.Any()) {
                        foreach (var error in state.Value.Errors) {
                            controller.ModelState.AddModelError(state.Key, error.ErrorMessage);
                        }
                    }
                }
            }
        }
    }
}