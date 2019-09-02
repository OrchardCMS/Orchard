using System;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Workflows.Helpers {
    public static class OutcomeSerializerExtensions {
        /// <summary>
        /// Returns a JSON formatted string.
        /// </summary>
        /// <param name="outcomesText">A comma separated string containing outcomes.</param>
        public static string FormatOutcomesJson(this string outcomesText, Localizer T) {
            var items = outcomesText != null 
                ? outcomesText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : Enumerable.Empty<string>();

            var query = 
                from item in items
                let outcome = HttpUtility.JavaScriptStringEncode(T.Encode(item).ToString())
                select "{Id:'" + outcome + "', Label:'" + outcome + "'}";
            
            var outcomes = String.Join(",", query);
            return outcomes;
        }
    }
}