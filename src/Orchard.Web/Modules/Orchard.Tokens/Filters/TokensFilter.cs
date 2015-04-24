using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Services;

namespace Orchard.Tokens.Filters.Services {

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
            if (_displayed == null) {
                return text;
            }

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // optimize code path if nothing to do
            if (!text.Contains("#{")) {
                return text;
            }

            Dictionary<string, object> data = new Dictionary<string, object>() { { "Content", _displayed } };

            text = _tokenizer.Replace(text, data);

            _displayed = null;

            return text;
        }
    }
}