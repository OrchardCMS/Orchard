using System;
using Orchard.Environment.Extensions;
using Orchard.Services;
using Orchard.Tokens;

namespace Orchard.Layouts.Filters {
    // TODO: Fix the version that lives in Orchard.Tokens.Filters.
    [OrchardFeature("Orchard.Layouts.Tokens")]
    public class TokensFilter : IHtmlFilter {

        private readonly ITokenizer _tokenizer;
 
        public TokensFilter(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        public string ProcessContent(string text, string flavor) {
            return TokensReplace(text);
        }

        private string TokensReplace(string text) {
            if (String.IsNullOrEmpty(text))
                return "";

            if (!text.Contains("#{")) {
                return text;
            }

            text = _tokenizer.Replace(text, null, new ReplaceOptions {Encoding = ReplaceOptions.NoEncode});

            return text;
        }
    }
}