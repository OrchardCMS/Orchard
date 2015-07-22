using System;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Models;
using Orchard.DynamicForms.Services;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.DynamicForms.Recipes.Executors {
    public class FormSubmissionsStep : RecipeExecutionStep {
        private readonly IFormService _formService;
        public FormSubmissionsStep(IFormService formService) {
            _formService = formService;
        }

        public override string Name {
            get { return "Forms"; }
        }
        public override void Execute(RecipeExecutionContext context) {
            var formsElement = context.RecipeStep.Step.Elements();
            foreach (var formElement in formsElement) {
                var formName = formElement.Attr<string>("Name");
                var submissionElements = formElement.Element("Submissions").Elements();

                foreach (var submissionElement in submissionElements) {
                    _formService.CreateSubmission(new Submission {
                        FormName = formName,
                        CreatedUtc = submissionElement.Attr<DateTime>("CreatedUtc"),
                        FormData = submissionElement.Value
                    });
                }
            }
        }
    }
}
