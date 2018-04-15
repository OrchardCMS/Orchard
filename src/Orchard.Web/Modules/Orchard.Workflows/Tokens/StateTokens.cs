using System;
using System.Linq;
using Orchard.Tokens;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Tokens {
    public class StateTokens : Component, ITokenProvider {

        public void Describe(DescribeContext context) {
            context.For("Workflow", T("Workflow"), T("Workflow tokens."))
                .Token("State:*", T("State:<workflowcontext path>"), T("The workflow context state to access. Workflow.State:MyData.MyProperty.SubProperty"))
                .Token("Token:*", T("Token:<token name>"), T("The workflow context token to access. Workflow.Token:MyToken"))
            ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<WorkflowContext>("Workflow")
                .Token(token => token.StartsWith("State:", StringComparison.OrdinalIgnoreCase) ? token.Substring("State:".Length) : null, ParseState)
                .Token(token => token.StartsWith("Token:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Token:".Length) : null, ParseToken);
        }

        /// <summary>
        /// Resolves the specified expression into an object stored in WorkflowContext.
        /// </summary>
        /// <param name="expression">The expression resolving to the state stored in WorkflowContext. E.g. "MyData.MyProperty.MySubProperty"</param>
        private object ParseState(string expression, WorkflowContext workflowContext) {
            var path = expression.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            var first = path.First();
            var obj = (dynamic)workflowContext.GetState(first);

            if (path.Length > 1) {
                foreach (var property in path.Skip(1)) {
                    obj = obj[property];
                }
            }

            return obj;
        }

        /// <summary>
        /// Returns the specified token from the token dictionary stored as part of the workflow context.
        /// </summary>
        private object ParseToken(string tokenName, WorkflowContext workflowContext) {
            return workflowContext.Tokens.ContainsKey(tokenName) ? workflowContext.Tokens[tokenName] : null;
        }
    }
}