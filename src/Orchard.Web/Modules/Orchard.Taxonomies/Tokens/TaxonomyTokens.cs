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
                   .Token(
                       token => {
                           var index = 0;
                           return (token.StartsWith("Terms:", StringComparison.OrdinalIgnoreCase) && int.TryParse(token.Substring("Terms:".Length), out index)) ? index.ToString() : null;
                       },
                       (token, t) => {
                           var index = Convert.ToInt32(token);
                           return index + 1 > t.Terms.Count() ? null : t.Terms.ElementAt(index).Name;
                       })
                // todo: extend Chain() in order to accept a filter like in Token() so that we can chain on an expression
                   .Chain("Terms:0", "Content", t => t.Terms.ElementAt(0))
                   .Chain("Terms:1", "Content", t => t.Terms.ElementAt(1))
                   .Chain("Terms:2", "Content", t => t.Terms.ElementAt(2))
                   .Chain("Terms:3", "Content", t => t.Terms.ElementAt(3))
                   ;
        }
    }
}