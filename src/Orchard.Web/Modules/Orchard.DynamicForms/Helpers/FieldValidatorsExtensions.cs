using System.Collections.Generic;
using System.Linq;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Helpers {
    public static class FieldValidatorsExtensions {
        public static IEnumerable<FieldValidatorSetting> Enabled(this IEnumerable<FieldValidatorSetting> list) {
            return list.Where(x => x.Enabled);
        }
    }
}