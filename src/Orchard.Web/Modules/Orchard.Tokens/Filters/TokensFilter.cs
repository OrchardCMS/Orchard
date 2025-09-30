using System;
using Orchard.Environment.Extensions;
using Orchard.Services;

namespace Orchard.Tokens.Filters {
    [OrchardFeature("Orchard.Tokens.HtmlFilter")]
    public class TokensFilter : HtmlFilter {

        private readonly ITokenizer _tokenizer;
 
        public TokensFilter(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }
        
        public override string ProcessContent(string text, HtmlFilterContext context) {
            return TokensReplace(text, context);
        }

        private string TokensReplace(string text, HtmlFilterContext context) {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            // Optimize code path if nothing to do.
            if (!text.Contains("#{"))
                return text;

            return _tokenizer.Replace(text, context.Data,
                String.Equals(context.Flavor, "html", StringComparison.OrdinalIgnoreCase)
                ? new ReplaceOptions { Encoding = ReplaceOptions.HtmlEncode }
                : new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
        }
    }
}