using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Services;

namespace Orchard.Tokens.Filters {

    [OrchardFeature("Orchard.Tokens.HtmlFilter")]
    public class TokensFilter : ContentHandler, IHtmlFilter {

        private readonly ITokenizer _tokenizer;
        private ContentItem _displayed;
 
        public TokensFilter(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        protected override void BuildDisplayShape(BuildDisplayContext context) {
            _displayed = context.ContentItem;
        }

        public string ProcessContent(string text, string flavor) {
            return TokensReplace(text);
        }

        private string TokensReplace(string text) {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            // Optimize code path if nothing to do.
            if (!text.Contains("#{")) {
                return text;
            }

            var data = new Dictionary<string, object>();

            if (_displayed != null)
                data["Content"] = _displayed;

            text = _tokenizer.Replace(text, data);

            _displayed = null;

            return text;
        }
    }
}