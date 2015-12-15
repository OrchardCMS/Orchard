using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Email.Forms {

    public class MailForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public MailForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => Shape.Form(
                Id: "ActionEmail",
                _Type: Shape.FieldSet(
                    Title: T("Send to"),
                    _RecipientOwner: Shape.Radio(
                        Id: "recipient-owner",
                        Name: "Recipient",
                        Value: "owner",
                        Title: T("Owner"),
                        Description: T("The owner of the content item in context, such as a blog post's author.")
                    ),
                    _RecipientAuthor: Shape.Radio(
                        Id: "recipient-author",
                        Name: "Recipient",
                        Value: "author",
                        Title: T("Author"),
                        Description: T("The current user when this action executes.")
                    ),
                    _RecipientAdmin: Shape.Radio(
                        Id: "recipient-admin",
                        Name: "Recipient",
                        Value: "admin",
                        Title: T("Site Admin"),
                        Description: T("The site administrator.")
                    ),
                    _RecipientOther: Shape.Radio(
                        Id: "recipient-other",
                        Name: "Recipient",
                        Value: "other",
                        Title: T("Other:")
                    ),
                    _OtherEmails: Shape.Textbox(
                        Id: "recipient-other-email",
                        Name: "RecipientOther",
                        Title: T("E-mail"),
                        Description: T("Specify a comma-separated list of e-mail recipients."),
                        Classes: new[] { "large", "text", "tokenized" }
                    )
                ),
                _Subject: Shape.Textbox(
                    Id: "Subject", Name: "Subject",
                    Title: T("Subject"),
                    Description: T("The subject of the e-mail."),
                    Classes: new[] { "large", "text", "tokenized" }),
                _Message: Shape.Textarea(
                    Id: "Body", Name: "Body",
                    Title: T("Body"),
                    Description: T("The body of the e-mail."),
                    Classes: new[] { "tokenized" }
                    )
                );

            context.Form("ActivityActionEmail", form);
        }
    }

    public class MailFormsValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != "ActivityActionEmail") return;

            var recipientFormValue = context.ValueProvider.GetValue("Recipient");
            var recipient = recipientFormValue != null ? recipientFormValue.AttemptedValue : String.Empty;

            if (recipient == String.Empty) {
                context.ModelState.AddModelError("Recipient", T("You must select at least one recipient").Text);
            }

            if (context.ValueProvider.GetValue("Subject").AttemptedValue == String.Empty) {
                context.ModelState.AddModelError("Subject", T("You must provide a Subject").Text);
            }

            if (context.ValueProvider.GetValue("Body").AttemptedValue == String.Empty) {
                context.ModelState.AddModelError("Body", T("You must provide a Body").Text);
            }

            if (context.ValueProvider.GetValue("RecipientOther").AttemptedValue == String.Empty && recipient == "other") {
                context.ModelState.AddModelError("RecipientOther", T("You must provide an e-mail address").Text);
            }
        }

        public void Validated(ValidatingContext context) {
        }
    }
}