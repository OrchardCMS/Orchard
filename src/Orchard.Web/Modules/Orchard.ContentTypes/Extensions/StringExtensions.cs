using System.Linq;
using System.Text.RegularExpressions;

namespace Orchard.ContentTypes.Extensions {
    public static class StrinExtensions {
        private static readonly Regex humps = new Regex("[A-Z][^A-Z]*");
        public static string CamelFriendly(this string camel) {
            return camel != null
                ? humps.Matches(camel).OfType<Match>().Select(m => m.Value).Aggregate((a, b) => a + " " + b).TrimStart(' ')
                : null;
        }

        public static string TrimEnd(this string rough, string trim = "") {
            if (rough == null)
                return null;

            return rough.EndsWith(trim)
                       ? rough.Substring(0, rough.Length - 4)
                       : rough;
        }
    }
}