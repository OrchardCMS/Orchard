using System;
using System.Linq;
using System.Web;

namespace Orchard.Workflows.Helpers {
    public static class OutcomeSerializerExtensions {
        /// <summary>
        /// Returns a JSON formatted string.
        /// </summary>
        /// <param name="outcomesText">A comma separated string containing outcomes.</param>
        public static string FormatOutcomesJson(this string outcomesText) {
            var items = outcomesText != null 
                ? outcomesText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : Enumerable.Empty<string>();

            var query = 
                from item in items
                let outcome = Encode(item)
                select "{label:'" + outcome + "', value:'" + outcome + "'}";
            
            var outcomes = String.Join(",", query);
            return outcomes;
        }

        private static string Encode(string value) {
            return HttpUtility.JavaScriptStringEncode(value);
        }
    }
}