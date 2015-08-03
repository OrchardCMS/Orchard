using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Models;
using Orchard.DynamicForms.Services;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.DynamicForms.Recipes.Executors {
    public class FormSubmissionsStep : RecipeExecutionStep {
        private readonly IFormService _formService;

        public FormSubmissionsStep(
            IFormService formService,
            RecipeExecutionLogger logger) : base(logger) {

            _formService = formService;
        }

        public override string Name {
            get { return "Forms"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            var formElements = context.RecipeStep.Step.Elements();
            foreach (var formElement in formElements) {
                var formName = formElement.Attr<string>("Name");
                Logger.Information("Importing form '{0}'.", formName);

                try {
                    var submissionElements = formElement.Element("Submissions").Elements().ToArray();
                    for (var i = 0; i < submissionElements.Length; i++) {
                        Logger.Information("Importing form submission {0}/{1}.", i + 1, submissionElements.Length);
                        var submissionElement = submissionElements[i];
                        _formService.CreateSubmission(new Submission {
                            FormName = formName,
                            CreatedUtc = submissionElement.Attr<DateTime>("CreatedUtc"),
                            FormData = submissionElement.Value
                        });
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing form '{0}'.", formName);
                    throw;
                }
            }
        }
    }
}
