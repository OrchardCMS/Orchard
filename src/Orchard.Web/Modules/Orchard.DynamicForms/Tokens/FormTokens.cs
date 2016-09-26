using System;
using Orchard.DynamicForms.Services.Models;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Tokens {
    public class FormTokens : Component, ITokenProvider {

        public void Describe(DescribeContext context) {
            context.For("FormSubmission", T("Dynamic Form submission"), T("Dynamic Form Submission tokens for use in workflows handling the Dynamic Form Submitted event."))
                .Token("Field:*", T("Field:<field name>"), T("The posted field value to access."))
                .Token("IsValid:*", T("IsValid:<field name>"), T("The posted field validation status."))
            ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<FormSubmissionTokenContext>("FormSubmission")
                .Token(token => token.StartsWith("Field:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Field:".Length) : null, GetFieldValue)
                .Token(token => token.StartsWith("IsValid:", StringComparison.OrdinalIgnoreCase) ? token.Substring("IsValid:".Length) : null, GetFieldValidationStatus);
        }

        private object GetFieldValue(string fieldName, FormSubmissionTokenContext context) {
            return context.PostedValues[fieldName];
        }

        private object GetFieldValidationStatus(string fieldName, FormSubmissionTokenContext context) {
            return context.ModelState.IsValidField(fieldName);
        }
    }
}