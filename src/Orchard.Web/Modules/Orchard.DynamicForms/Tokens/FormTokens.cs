using System;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;
using Orchard.Layouts.Framework.Elements;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Tokens {
    public class FormTokens : Component, ITokenProvider {

        public void Describe(DescribeContext context) {
            context.For("FormSubmission", T("Dynamic Form submission"), T("Dynamic Form Submission tokens for use in workflows handling the Dynamic Form Submitted event."))
                .Token("Field:*", T("Field:<field name>"), T("The posted field value to access."), "Text")
                .Token("IsValid:*", T("IsValid:<field name>"), T("The posted field validation status."))
                .Token("FormName", T("FormName"), T("The posted form's name."));
            context.For("Element", T("Element"), T("Form Element tokens to get the content that is going to be edited through a dynamic form."))
                .Token("ContentToEdit", T("ContentToEdit"), T("The content item to edit through a form."));            
        }

        public void Evaluate(EvaluateContext context) {
            context.For<FormSubmissionTokenContext>("FormSubmission")
                .Token(token => token.StartsWith("Field:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Field:".Length) : null, GetFieldValue)
                .Chain(FilterChainParam, "Text", GetFieldValue)
                .Token(token => token.StartsWith("IsValid:", StringComparison.OrdinalIgnoreCase) ? token.Substring("IsValid:".Length) : null, GetFieldValidationStatus)
            context.For<Element>("Element")
                .Token("ContentToEdit", GetContentToEdit)
                .Chain("ContentToEdit", "Content", GetContentToEdit);
        }

        private object GetContentToEdit(Element element) {
            var formElement = element as FormElement;
            if (formElement != null)
                return formElement.Form.ContentItemToEdit;
            while (element!=null) {
                element = element.Container;
                formElement = element as FormElement;
                if (formElement != null)
                    return formElement.Form.ContentItemToEdit;
            }
            return null;
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
        private string GetFormName(FormSubmissionTokenContext context)
        {
            return context.Form.Name;
        }
        private object GetFieldValidationStatus(string fieldName, FormSubmissionTokenContext context) {
            return context.ModelState.IsValidField(fieldName);
        }
    }
}