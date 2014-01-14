using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Features;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Email.Forms {
    public class EmailForm : Component, IFormProvider {
        private readonly IFeatureManager _featureManager;
        protected dynamic New { get; set; }

        public EmailForm(IShapeFactory shapeFactory,
            IFeatureManager featureManager) {
            _featureManager = featureManager;
            New = shapeFactory;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> formFactory =
                shape => {
                    var jobsQueueEnabled = _featureManager.GetEnabledFeatures().Any(x => x.Id == "Orchard.JobsQueue");

                    var form = New.Form(
                        Id: "EmailActivity",
                        _Type: New.FieldSet(
                            Title: T("Send to"),
                            _Recipients: New.Textbox(
                                Id: "recipients",
                                Name: "Recipients",
                                Title: T("Email Addresses"),
                                Description: T("Specify a comma-separated list of recipient email addresses. To include a display name, use the following format: John Doe &lt;john.doe@outlook.com&gt;"),
                                Classes: new[] {"large", "text", "tokenized"}),
                            _Subject: New.Textbox(
                                Id: "Subject", Name: "Subject",
                                Title: T("Subject"),
                                Description: T("The subject of the email message."),
                                Classes: new[] {"large", "text", "tokenized"}),
                            _Message: New.Textarea(
                                Id: "Body", Name: "Body",
                                Title: T("Body"),
                                Description: T("The body of the email message."),
                                Classes: new[] {"tokenized"})
                            ));

                    if (jobsQueueEnabled) {
                        form._Type._Queued(New.Checkbox(
                                Id: "Queued", Name: "Queued",
                                Title: T("Queued"),
                                Checked: false, Value: "true",
                                Description: T("Check send it as a queued job.")));

                        form._Type._Priority(New.SelectList(
                                Id: "priority",
                                Name: "Priority",
                                Title: T("Priority"),
                                Description: ("The priority of this message.")
                            ));

                        form._Type._Priority.Add(new SelectListItem { Value = "-50", Text = T("Low").Text });
                        form._Type._Priority.Add(new SelectListItem { Value = "0", Text = T("Normal").Text });
                        form._Type._Priority.Add(new SelectListItem { Value = "50", Text = T("High").Text });
                    }

                    return form;
                };

            context.Form("EmailActivity", formFactory);
        }

    }

    public class EmailFormValidator : IFormEventHandler {
        public Localizer T { get; set; }
        public void Building(BuildingContext context) {}
        public void Built(BuildingContext context) {}
        public void Validated(ValidatingContext context) { }

        public void Validating(ValidatingContext context) {
            if (context.FormName != "EmailActivity") return;

            var recipients = context.ValueProvider.GetValue("Recipients").AttemptedValue;
            var subject = context.ValueProvider.GetValue("Subject").AttemptedValue;
            var body = context.ValueProvider.GetValue("Body").AttemptedValue;

            if (String.IsNullOrWhiteSpace(recipients)) {
                context.ModelState.AddModelError("Recipients", T("You must specify at least one recipient.").Text);
            }

            if (String.IsNullOrWhiteSpace(subject)) {
                context.ModelState.AddModelError("Subject", T("You must provide a Subject.").Text);
            }

            if (String.IsNullOrWhiteSpace(body)) {
                context.ModelState.AddModelError("Body", T("You must provide a Body.").Text);
            }
        }
    }
}