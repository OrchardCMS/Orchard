using System;
using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Localization;
using Orchard.Tokens;

namespace Orchard.Taxonomies.Tokens {
    public class TaxonomyTokens : ITokenProvider {

        public TaxonomyTokens() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            // Usage:
            // Content.Fields.Article.Categories.Terms -> 'Science, Sports, Arts'
            // Content.Fields.Article.Categories.Terms:0 -> 'Science'

            // When used with an indexer, it can be chained with Content tokens
            // Content.Fields.Article.Categories.Terms:0.DisplayUrl -> http://...

            context.For("TaxonomyField", T("Taxonomy Field"), T("Tokens for Taxonomy Fields"))
                   .Token("Terms", T("Terms"), T("The terms (Content) associated with field."))
                   .Token("Terms[:*]", T("Terms"), T("A term by its index. Can be chained with Content tokens."))
                   ;
        }

        public void Evaluate(EvaluateContext context) {

            context.For<TaxonomyField>("TaxonomyField")
                   .Token("Terms", field => String.Join(", ", field.Terms.Select(t => t.Name).ToArray()))
                   .Token(FilterTokenParam,
                       (index, field) => {
                           var term = field.Terms.ElementAtOrDefault(Convert.ToInt32(index));
                           return term != null ? term.Name : null;
                       })
                   .Chain(FilterChainParam, "Content", (index, field) => field.Terms.ElementAtOrDefault(Convert.ToInt32(index)))
                   ;
        }

        private static string FilterTokenParam(string token) {
            int index = 0;
            return (token.StartsWith("Terms:", StringComparison.OrdinalIgnoreCase) && int.TryParse(token.Substring("Terms:".Length), out index)) ? index.ToString() : null;
        }

        private static Tuple<string, string> FilterChainParam(string token) {
            int tokenLength = "Terms:".Length;
            int index = 0;
            int chainIndex = token.IndexOf('.');
            if (!token.StartsWith("Terms:", StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength)
                return null;

            if (int.TryParse(token.Substring(tokenLength, chainIndex - tokenLength), out index)) {
                return new Tuple<string, string>(index.ToString(), token.Substring(chainIndex + 1));
            }
            else {
                return null;
            }
        }
    }
}