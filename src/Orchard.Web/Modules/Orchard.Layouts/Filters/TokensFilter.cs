using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Services;
using Orchard.Tokens;

namespace Orchard.Layouts.Filters {
    [OrchardFeature("Orchard.Layouts.Tokens")]
    public class TokensFilter : IElementFilter {

        private readonly ITokenizer _tokenizer;
 
        public TokensFilter(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        public string ProcessContent(string text, string flavor) {
            return ProcessContent(text, flavor, new Dictionary<string, object>());
        }

        public string ProcessContent(string text, string flavor, IDictionary<string, object> context) {
            if (String.IsNullOrEmpty(text))
                return "";

            if (!text.Contains("#{")) {
                return text;
            }
            
            text = _tokenizer.Replace(text, context, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

            return text;
        }
    }
}