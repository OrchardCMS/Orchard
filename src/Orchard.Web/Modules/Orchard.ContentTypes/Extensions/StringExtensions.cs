using System.Linq;
using System.Text.RegularExpressions;

namespace Orchard.ContentTypes.Extensions {
    public static class StrinExtensions {
        private static readonly Regex humps = new Regex("[A-Z][^A-Z]*");
        public static string CamelFriendly(this string camel) {
            if (camel == null)
                return null;

            var matches = humps.Matches(camel).OfType<Match>().Select(m => m.Value);
            if (matches.Any()) {
                return matches.Aggregate((a, b) => a + " " + b).TrimStart(' ');
            }
            else {
                return camel;
            }
        }

        public static string TrimEnd(this string rough, string trim = "") {
            if (rough == null)
                return null;

            return rough.EndsWith(trim)
                       ? rough.Substring(0, rough.Length - trim.Length)
                       : rough;
        }
    }
}