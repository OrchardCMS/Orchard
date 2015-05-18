﻿using System;
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
                .Token("TrimEnd:*", T("TrimEnd:<chars|number>"), T("Trims the specified characters or number of them from the end of the string."), "Text")
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
                .Token(token => token.StartsWith("TrimEnd:", StringComparison.OrdinalIgnoreCase) ? token.Substring("TrimEnd:".Length) : null, TrimEnd)
                .Token("UrlEncode", HttpUtility.UrlEncode)
                .Token("HtmlEncode", HttpUtility.HtmlEncode)
                .Token("LineEncode", text => text.Replace(System.Environment.NewLine, "<br />"))
                ;
                
        }

        private static string TrimEnd(string token, string param) {
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