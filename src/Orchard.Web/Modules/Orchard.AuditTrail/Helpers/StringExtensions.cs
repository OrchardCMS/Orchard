using System.Text.RegularExpressions;
using Orchard.Localization;

namespace Orchard.AuditTrail.Helpers
{
    public static class StringExtensions
    {
        public static int? ToInt32(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            int i;
            if (!int.TryParse(value, out i))
                return null;

            return i;
        }

        public static string OrIfEmpty(this string value, LocalizedString emptyString)
        {
            return string.IsNullOrWhiteSpace(value) ? emptyString.Text : value;
        }

        public static string NewlinesToHtml(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? value : Regex.Replace(value, @"\n", "<br/>");
        }
    }
}