using System;
using Orchard.Localization;

namespace Orchard.Tokens.Providers {
    public class RequestTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public RequestTokens(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Request", T("Http Request"), T("Current Http Request tokens."))
                .Token("QueryString:*", T("QueryString:<element>"), T("The Query String value for the specified element."))
                .Token("Form:*", T("Form:<element>"), T("The Form value for the specified element."))
            ;
        }

        public void Evaluate(EvaluateContext context) {
            if (_workContextAccessor.GetContext().HttpContext == null) {
                return;
            }

            context.For("Request", _workContextAccessor.GetContext().HttpContext.Request)
                .Token(
                    token => token.StartsWith("QueryString:", StringComparison.OrdinalIgnoreCase) ? token.Substring("QueryString:".Length) : null,
                    (token, request) => request.QueryString.Get(token)
                )
                .Token(
                    token => token.StartsWith("Form:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Form:".Length) : null,
                    (token, request) => request.Form.Get(token)
                );
        }
    }
}