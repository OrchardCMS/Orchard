using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Scripting.CSharp.Forms {
    public class DecisionForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public DecisionForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
              shape => Shape.Form(
                Id: "ActionDecision",
                _Message: Shape.Textbox(
                  Id: "outcomes", Name: "Outcomes",
                  Title: T("Possible Outcomes."),
                  Description: T("A comma-separated list of possible outcomes."),
                  Classes: new[] { "text medium" }),
                _Script: Shape.TextArea(
                  Id: "Script", Name: "Script",
                  Title: T("Script"),
                  Description: T("The script to run every time the Decision Activity is invoked. You can use ContentItem, Services, WorkContext, Workflow and T(). Call SetOutcome(string outcome) to define the outcome of the activity."),
                  Classes: new[] { "tokenized" }
                  )
                );

            context.Form("ActivityActionDecision", form);
        }

    }

    public class DecisionFormsValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName == "ActivityActionDecision") {
                if (context.ValueProvider.GetValue("Script").AttemptedValue == string.Empty) {
                    context.ModelState.AddModelError("Script", T("You must provide a Script").Text);
                }
            }
        }

        public void Validated(ValidatingContext context) {

        }
    }

}
