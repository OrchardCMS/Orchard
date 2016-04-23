using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;
using Orchard.Taxonomies.Models;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Helpers {
    public static class EnumerableTermPartExtensions {
        public static IEnumerable<SelectListItem> GetSelectListItems(this IEnumerable<TermPart> terms, Taxonomy taxonomy, ITokenizer tokenizer, IEnumerable<string> runtimeValues = null) {
            var valueExpression = !String.IsNullOrWhiteSpace(taxonomy.ValueExpression) ? taxonomy.ValueExpression : "{Content.Id}";
            var textExpression = !String.IsNullOrWhiteSpace(taxonomy.TextExpression) ? taxonomy.TextExpression : "{Content.DisplayText}";

            var projection = terms.Select(x => {
                var data = new { Content = x };
                var value = tokenizer.Replace(valueExpression, data);
                var text = tokenizer.Replace(textExpression, data);

                return new SelectListItem {
                    Text = text,
                    Value = value,
                    Selected = runtimeValues!=null && runtimeValues.Contains(value, StringComparer.OrdinalIgnoreCase)
                };
            });

            switch (taxonomy.SortOrder) {
                case "Asc":
                    projection = projection.OrderBy(x => x.Text);
                    break;
                case "Desc":
                    projection = projection.OrderByDescending(x => x.Text);
                    break;
            }

            return projection;
        }

    }
}