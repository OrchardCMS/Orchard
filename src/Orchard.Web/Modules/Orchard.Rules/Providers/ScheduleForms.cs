using System;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Rules.Models;
using System.Linq;

namespace Orchard.Rules.Providers {
    public class ScheduleForms : IFormProvider {
        private readonly IRepository<RuleRecord> _repository;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ScheduleForms(IShapeFactory shapeFactory, IRepository<RuleRecord> repository) {
            _repository = repository;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("ActionDelay",
                shape => {
                    var rules = _repository.Table.OrderBy(x => x.Name).ToList();

                    var form = Shape.Form(
                        Id: "ActionDelay",
                        _Amount: Shape.Textbox(
                            Id: "Amount", Name: "Amount",
                            Title: T("Amount"),
                            Classes: new[] { "text-small" }),
                        _Type: Shape.SelectList(
                            Id: "Unity", Name: "Unity",
                            Title: T("Amount type"))
                            .Add(new SelectListItem { Value = "Minute", Text = T("Minutes").Text, Selected = true })
                            .Add(new SelectListItem { Value = "Hour", Text = T("Hours").Text })
                            .Add(new SelectListItem { Value = "Day", Text = T("Days").Text })
                            .Add(new SelectListItem { Value = "Week", Text = T("Weeks").Text })
                            .Add(new SelectListItem { Value = "Month", Text = T("Months").Text }),
                        _Rule: Shape.SelectList(
                            Id: "RuleId", Name: "RuleId",
                            Title: T("Rule to trigger"))
                        );

                    foreach (var rule in rules) {
                        form._Rule.Add(new SelectListItem { Value = rule.Id.ToString(), Text = rule.Name });
                    }

                    return form;
                }
            );

            context.Form("ActionSchedule",
                shape => Shape.Form(
                Id: "ActionSchedule",
                _Date: Shape.Textbox(
                    Id: "Date", Name: "Date",
                    Title: T("Date")),
                _Time: Shape.Textbox(
                    Id: "Time", Name: "Time",
                    Title: T("Time"))
                )
            );
        }
    }

    public class ScheduleFormsValitator : FormHandler {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context) {
            if (context.FormName == "ActionDelay") {
                if (context.ValueProvider.GetValue("Amount").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Amount", T("You must provide an Amount").Text);
                }

                if (context.ValueProvider.GetValue("Unity").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Unity", T("You must provide a Type").Text);
                }

                if (context.ValueProvider.GetValue("RuleId").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("RuleId", T("You must select at least one Rule").Text);
                }
            }
        }
    }
}