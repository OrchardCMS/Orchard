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
                .Token("Format:*", T("Format:<text format>"), T("Optional format specifier (e.g. foo{0}bar). See format strings at <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/az4se3k1.aspx\">Standard Formats</a> and <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx\">Custom Formats</a>"), "DateTime")
                .Token("UrlEncode", T("Url Encode"), T("Encodes a URL string."), "Text")
                .Token("HtmlEncode", T("Html Encode"), T("Encodes an HTML string."), "Text")
                .Token("LineEncode", T("Line Encode"), T("Replaces new lines with <br /> tags."))
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<String>("Text", () => "")
                .Token(  // {Text}
                    token => token == String.Empty ? String.Empty : null,
                    (token, d) => d.ToString())
                .Token( // {Text.Limit:<length>[,<ellipsis>]}
                    token => {
                        if (token.StartsWith("Limit:", StringComparison.OrdinalIgnoreCase)) {
                            var param = token.Substring("Limit:".Length);
                            return param;
                        }
                        return null;
                    },
                    (token, t) => Limit(t, token))
                // {Text.Format:<formatstring>}
                .Token(
                    token => token.StartsWith("Format:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Format:".Length) : null,
                    (token, d) => String.Format(d,token))
                .Token("UrlEncode", HttpUtility.UrlEncode)
                .Token("HtmlEncode", HttpUtility.HtmlEncode)
                .Token("LineEncode", text => text.Replace(System.Environment.NewLine, "<br />"))
                ;
                
        }

        private string Limit(string token, string param) {
            if(String.IsNullOrEmpty(token)) {
                return String.Empty;
            }

            var index = param.IndexOf(',');

            // no ellipsis
            if (index == -1) {
                var limit = Int32.Parse(param);
                token = token.Substring(0, limit);
            }
            else {
                var limit = Int32.Parse(param.Substring(0, index));
                var ellipsis = param.Substring(index + 1);
                token = token.Substring(0, limit) + ellipsis;
            }

            return token;
        }

    }
}