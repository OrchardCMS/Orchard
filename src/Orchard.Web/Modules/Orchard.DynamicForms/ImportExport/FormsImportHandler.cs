using System;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Models;
using Orchard.DynamicForms.Services;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.DynamicForms.ImportExport {
    [OrchardFeature("Orchard.DynamicForms.ImportExport")]
    public class FormsImportHandler : Component, IRecipeHandler {
        private readonly IFormService _formService;
        public FormsImportHandler(IFormService formService) {
            _formService = formService;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Forms", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var formsElement = recipeContext.RecipeStep.Step.Elements();
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

            recipeContext.Executed = true;
        }
    }
}
