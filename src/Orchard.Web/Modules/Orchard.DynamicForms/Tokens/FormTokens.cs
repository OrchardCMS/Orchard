using System;
using Orchard.DynamicForms.Services.Models;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Tokens {
    public class FormTokens : Component, ITokenProvider {

        public void Describe(DescribeContext context) {
            context.For("FormSubmission", T("Dynamic Form submission"), T("Dynamic Form Submission tokens for use in workflows handling the Dynamic Form Submitted event."))
                .Token("Field:*", T("Field:<field name>"), T("The posted field value to access."), "Text")
                .Token("IsValid:*", T("IsValid:<field name>"), T("The posted field validation status."))
                .Token("CreatedContent", T("CreatedContent"), T("Id of the Content Item created by the form."))
            ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<FormSubmissionTokenContext>("FormSubmission")
                .Token(token => token.StartsWith("Field:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Field:".Length) : null, GetFieldValue)
                .Chain(FilterChainParam, "Text", GetFieldValue)
                .Token(token => token.StartsWith("IsValid:", StringComparison.OrdinalIgnoreCase) ? token.Substring("IsValid:".Length) : null, GetFieldValidationStatus)
                .Token("CreatedContent", GetCreatedContent)
                .Chain("CreatedContent", "Content", GetCreatedContent);
        }

        private static Tuple<string, string> FilterChainParam(string token) {
            int tokenLength = "Field:".Length;
            int chainIndex = token.IndexOf('.');
            if (token.StartsWith("Field:", StringComparison.OrdinalIgnoreCase) && chainIndex > tokenLength)
                return new Tuple<string, string>(token.Substring(tokenLength, chainIndex - tokenLength), token.Substring(chainIndex + 1));
            else
                return null;
        }

        private string GetFieldValue(string fieldName, FormSubmissionTokenContext context) {
            return context.PostedValues[fieldName];
        }

        private object GetFieldValidationStatus(string fieldName, FormSubmissionTokenContext context) {
            return context.ModelState.IsValidField(fieldName);
        }

        private object GetCreatedContent(FormSubmissionTokenContext context)
        {
            return context.CreatedContent;
        }
    }
}
