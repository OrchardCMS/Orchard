using System;
using System.Web;
using Orchard.Localization;

namespace Orchard.Tokens.Providers {
    public class TextTokens : ITokenProvider {
        public TextTokens() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Text", T("Text"), T("Tokens for text strings"))
                .Token("Limit:*", T("Limit:<text length>[,<ellipsis>]"), T("Limit text to specified length and append an optional ellipsis text."))
                .Token("Format:*", T("Format:<text format>"), T("Optional format specifier (e.g. foo{0}bar)."))
                .Token("TrimEnd:*", T("TrimEnd:<chars|number>"), T("Trims the specified characters or number of them from the end of the string."))
                .Token("UrlEncode", T("Url Encode"), T("Encodes a URL string."), "Text")
                .Token("HtmlEncode", T("Html Encode"), T("Encodes an HTML string."), "Text")
                .Token("JavaScriptEncode", T("JavaScript Encode"), T("Encodes a JavaScript string."), "Text")
                .Token("LineEncode", T("Line Encode"), T("Replaces new lines with <br /> tags."), "Text")
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<String>("Text", () => "")
                // {Text}
                .Token(
                    token => token == String.Empty ? String.Empty : null,
                    (token, d) => d.ToString())
                // {Text.Limit:<length>[,<ellipsis>]}
                .Token(
                    token => FilterTokenParam("Limit:", token),
                    (token, t) => Limit(t, token))
                // {Text.Format:<formatstring>}
                .Token(
                    token => FilterTokenParam("Format:", token),
                    (token, d) => String.Format(token, d))
                // {Text.TrimEnd:<chars|number>}
                .Token(token => FilterTokenParam("TrimEnd:", token), TrimEnd)
                .Token("UrlEncode", HttpUtility.UrlEncode)
                .Chain("UrlEncode", "Text", HttpUtility.UrlEncode)
                .Token("HtmlEncode", HttpUtility.HtmlEncode)
                .Chain("HtmlEncode", "Text", HttpUtility.HtmlEncode)
                .Token("JavaScriptEncode", HttpUtility.JavaScriptStringEncode)
                .Chain("JavaScriptEncode", "Text", HttpUtility.JavaScriptStringEncode)
                .Token("LineEncode", text => text.Replace(System.Environment.NewLine, "<br />"))
                .Chain("LineEncode", "Text", text => text.Replace(System.Environment.NewLine, "<br />"))
                ;

        }

        private static string FilterTokenParam(string tokenName, string token) {
            return token.StartsWith(tokenName, StringComparison.OrdinalIgnoreCase) ? token.Substring(tokenName.Length) : null;
        }

        private static string TrimEnd(string param, string token) {
            if (String.IsNullOrEmpty(param)) {
                return token;
            }

            int n;
            if (!int.TryParse(param, out n)) {
                return token.TrimEnd(param.ToCharArray());
            }

            n = Math.Max(0, n); // prevent negative numbers
            return token.Substring(0, token.Length - n);
        }

        private static string Limit(string token, string param) {
            if(String.IsNullOrEmpty(token)) {
                return String.Empty;
            }

            int index = param.IndexOf(',');
            int limit = index == -1 ? Int32.Parse(param) : Int32.Parse(param.Substring(0, index));

            if (token.Length <= limit) {
                // no limit
                return token;
            }
            if (index == -1) {
                // no ellipsis
                return token.Substring(0, limit);
            }
            else {
                // ellipsis
                var ellipsis = param.Substring(index + 1);
                return token.Substring(0, limit) + ellipsis;
            }
        }

    }
}