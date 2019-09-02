using System;
using System.Linq;
using System.Web;
using Orchard.Localization;

namespace Orchard.Tokens.Providers {
    public class TextTokens : ITokenProvider {
        private static string[] _textChainableTokens;
        public TextTokens() {
            T = NullLocalizer.Instance;
            _textChainableTokens = new string[] { "Limit", "Format", "TrimEnd", "TrimStart" };

        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Text", T("Text"), T("Tokens for text strings"))
                .Token("Limit:*", T("Limit:<text length>[,<ellipsis>]"), T("Limit text to specified length and append an optional ellipsis text."))
                .Token("Format:*", T("Format:<text format>"), T("Optional format specifier (e.g. foo{0}bar)."))
                .Token("TrimEnd:*", T("TrimEnd:<chars|number>"), T("Trims the specified characters or number of them from the end of the string."))
                .Token("TrimStart:*", T("TrimStart:<chars|number>"), T("Trims the specified characters or number of them from the beginning of the string."))
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
                    (token, d) => d)
                // {Text.Limit:<length>[,<ellipsis>]}
                .Token(
                    token => FilterTokenParam("Limit:", token),
                    (token, t) => Limit(t, token))
                .Chain(FilterChainLimitParam, "Text", (token, t) => Limit(t, token))
                // {Text.Format:<formatstring>}
                .Token(
                    token => FilterTokenParam("Format:", token),
                    (token, d) => String.Format(token, d))
                .Chain(FilterChainFormatParam, "Text", (token, d) => String.Format(token, d))
                // {Text.TrimEnd:<chars|number>}
                .Token(token => FilterTokenParam("TrimEnd:", token), TrimEnd)
                .Chain(FilterChainTrimEndParam, "Text", TrimEnd)
                // {Text.TrimStart:<chars|number>}
                .Token(token => FilterTokenParam("TrimStart:", token), TrimStart)
                .Chain(FilterChainTrimStartParam, "Text", TrimStart)
                .Token("UrlEncode", HttpUtility.UrlEncode)
                .Chain("UrlEncode", "Text", HttpUtility.UrlEncode)
                .Token("HtmlEncode", HttpUtility.HtmlEncode)
                .Chain("HtmlEncode", "Text", HttpUtility.HtmlEncode)
                .Token("JavaScriptEncode", HttpUtility.JavaScriptStringEncode)
                .Chain("JavaScriptEncode", "Text", HttpUtility.JavaScriptStringEncode)
                .Token("LineEncode", text => ReplaceNewLineCharacters(text))
                .Chain("LineEncode", "Text", text => ReplaceNewLineCharacters(text))
                ;

        }

        private static string ReplaceNewLineCharacters(string text) {
            return text
                    .Replace("\r\n", "<br />")
                    .Replace("\r", "<br />")
                    .Replace("\n", "<br />");
        }

        private Tuple<string, string> FilterChainLimitParam(string token) {
            return FilterChainParam("Limit:", token);
        }

        private Tuple<string, string> FilterChainFormatParam(string token) {
            return FilterChainParam("Format:", token);
        }

        private Tuple<string, string> FilterChainTrimEndParam(string token) {
            return FilterChainParam("TrimEnd:", token);
        }

        private Tuple<string, string> FilterChainTrimStartParam(string token) {
            return FilterChainParam("TrimStart:", token);
        }

        private static string FilterTokenParam(string tokenName, string token) {
            if (!token.StartsWith(tokenName, StringComparison.OrdinalIgnoreCase)) return null;
            string tokenPrefix;
            int chainIndex, tokenLength;
            if (token.IndexOf(":") == -1) {
                tokenPrefix = token;
            }
            else {
                tokenPrefix = token.Substring(0, token.IndexOf(":"));
            }
            if (!_textChainableTokens.Contains(tokenPrefix, StringComparer.OrdinalIgnoreCase)) {
                return token.StartsWith(tokenName, StringComparison.OrdinalIgnoreCase) ? token.Substring(tokenName.Length) : null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = (tokenPrefix + ":").Length;
            if (chainIndex == 0) {// ")." has not be found
                return token.Substring(tokenLength).Trim(new char[] { '(', ')' });
            }
            if (!token.StartsWith((tokenPrefix + ":"), StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength) {
                return null;
            }
            return token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenName">The name of the Token (e.g. "Limit:")</param>
        /// <param name="token">The entire Token (e.g. "Limit:(3).TrimStart:4")</param>
        /// <returns>Tuple representing the token parameter and next tokens (e.g. for token "Limit:(3).TrimStart:4", first string have to be "3", second one have to be "TrimStart:4")</returns>
        private static Tuple<string, string> FilterChainParam(string tokenName, string token) {
            if (!token.StartsWith(tokenName, StringComparison.OrdinalIgnoreCase)) return null;
            string tokenPrefix;
            int chainIndex, tokenLength;

            if (token.IndexOf(":") == -1) {
                tokenPrefix = token;
            }
            else {
                tokenPrefix = token.Substring(0, token.IndexOf(":"));
            }
            if (!_textChainableTokens.Contains(tokenPrefix, StringComparer.OrdinalIgnoreCase)) {
                return new Tuple<string, string>(token, token);
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = (tokenPrefix + ":").Length;
            if (chainIndex == 0) { // ")." has not be found
                return new Tuple<string, string>(token.Substring(tokenLength).Trim(new char[] { '(', ')' }), "");
            }
            if (!token.StartsWith((tokenPrefix + ":"), StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength) {
                return null;
            }
            return new Tuple<string, string>(token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' }), token.Substring(chainIndex + 1));

        }

        private static string TrimStart(string param, string token) {
            if (string.IsNullOrEmpty(param)) {
                return token;
            }

            int n;
            if (!int.TryParse(param, out n)) {
                return token.TrimStart(param.ToCharArray());
            }

            n = Math.Max(0, n); // prevent negative numbers
            return token.Substring(n);
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
            if (String.IsNullOrEmpty(token)) {
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