using System;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Workflows.Forms {
    [OrchardFeature("Orchard.Workflows.Timer")]
    public class TimerForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public TimerForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("ActivityTimer",
                shape => {
                    var form = Shape.Form(
                        Id: "ActionDelay",
                        _Amount: Shape.Textbox(
                            Id: "Amount", Name: "Amount",
                            Title: T("Amount"),
                            Description: T("Amount of time units to add."),
                            Classes: new[] { "text small tokenized" }),
                        _Type: Shape.SelectList(
                            Id: "Unity", Name: "Unity",
                            Title: T("Amount type"))
                            .Add(new SelectListItem { Value = "Minute", Text = T("Minutes").Text, Selected = true })
                            .Add(new SelectListItem { Value = "Hour", Text = T("Hours").Text })
                            .Add(new SelectListItem { Value = "Day", Text = T("Days").Text })
                            .Add(new SelectListItem { Value = "Week", Text = T("Weeks").Text })
                            .Add(new SelectListItem { Value = "Month", Text = T("Months").Text }),
                        _Date: Shape.Textbox(
                            Id: "Date", Name: "Date",
                            Title: T("Date"),
                            Description: T("Optional. Starting date/time to calculate difference from. Leave blank to use current date/time."),
                            Classes: new[] {"text medium tokenized"}));

                    return form;
                }
            );
        }
    }

    public class ScheduleFormsValitator : FormHandler {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context) {
            if (context.FormName == "ActivityTimer") {
                if (context.ValueProvider.GetValue("Amount").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Amount", T("You must provide an Amount").Text);
                }

                if (context.ValueProvider.GetValue("Unity").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Unity", T("You must provide a Type").Text);
                }
            }
        }
    }
}