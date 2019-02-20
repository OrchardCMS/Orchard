using System.Linq;
using System.Web;
using Orchard.Localization;

namespace Orchard.Localization {

    /// <summary>
    /// Localizes some text based on the current Work Context culture
    /// </summary>
    /// <param name="text">The text format to localize</param>
    /// <param name="args">The arguments used in the text format. The arguments are HTML-encoded if they don't implement <see cref="System.Web.IHtmlString"/>.</param>
    /// <returns>An HTML-encoded localized string</returns>
    public delegate LocalizedString Localizer(string text, params object[] args);
}

namespace Orchard.Mvc.Html {
    public static class LocalizerExtensions {
        public static LocalizedString Plural(this Localizer T, string textSingular, string textPlural, int count, params object[] args) {
            return T(count == 1 ? textSingular : textPlural, new object[] { count }.Concat(args).ToArray());
        }

        public static LocalizedString Plural(this Localizer T, string textNone, string textSingular, string textPlural, int count, params object[] args) {
            switch (count) {
                case 0:
                    return T(textNone, new object[] {count}.Concat(args).ToArray());
                case 1:
                    return T(textSingular, new object[] {count}.Concat(args).ToArray());
                default:
                    return T(textPlural, new object[] {count}.Concat(args).ToArray());
            }
        }

        public static LocalizedString Encode(this Localizer T, string unsecureText) {
            return T(HttpUtility.HtmlEncode(unsecureText));
        }
    }
}