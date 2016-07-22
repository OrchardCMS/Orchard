using Orchard.Localization;
using System.Collections.Generic;

namespace Orchard.ContentManagement {
    public static class UpdateModelExtensions {
        public static void AddModelErrors(this IUpdateModel updateModel, IDictionary<string, LocalizedString> validationErrors) {
            foreach (var error in validationErrors) {
                updateModel.AddModelError(error.Key, error.Value);
            }
        }
    }
}