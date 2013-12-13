using System;
using Orchard.DisplayManagement;
using Orchard.Templates.Services;
using Orchard.Tokens;

namespace Orchard.Templates.Tokens {
    public class ShapeTokenProvider : Component, ITokenProvider {
        private readonly ITemplateService _templateService;

        public ShapeTokenProvider(ITemplateService templateService) {
            _templateService = templateService;
        }

        public void Describe(DescribeContext context) {
            context.For("Shape", T("Shape"), T("Shape tokens."))
                .Token("Execute:*", T("Execute:<shape name>"), T("Executes the specified shape."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For("Shape", "")
                .Token(t => t.StartsWith("Execute:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Execute:".Length) : null, (shapeName, data) => TokenValue(context, shapeName, data));
        }

        private object TokenValue(EvaluateContext context, string shapeName, string data) {
            var parameters = Arguments.From(context.Data.Values, context.Data.Keys);
            return _templateService.ExecuteShape(shapeName, parameters);
        }
    }
}