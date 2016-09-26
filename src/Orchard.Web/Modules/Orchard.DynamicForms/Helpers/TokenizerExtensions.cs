using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Helpers {
    public static class TokenizerExtensions {
        public static IEnumerable<SelectListItem> Replace(this ITokenizer tokenizer, IEnumerable<SelectListItem> items) {
            return items.Select(item => new SelectListItem {
                Text = tokenizer.Replace(item.Text, null),
                Value = item.Value,
                Disabled = item.Disabled,
                Group = item.Group,
                Selected = item.Selected
            });
        }
    }
}