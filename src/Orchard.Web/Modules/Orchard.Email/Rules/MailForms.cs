using System;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Email.Rules {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    [OrchardFeature("Orchard.Email.Rules")]
    public class MailForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public MailForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
        }

        public void Describe(dynamic context) {
            Func<IShapeFactory, dynamic> form = 
                shape => Shape.Form(
                Id: "ActionEmail",
                _Type: Shape.SelectList(
                    Id: "Recipient", Name: "Recipient",
                    Title: T("Send to"),
                    Description: T("Select who should be the recipient of this e-mail."))
                    .Add(new SelectListItem { Value = "owner", Text = T("Owner").Text })
                    .Add(new SelectListItem { Value = "author", Text = T("Author").Text })
                    .Add(new SelectListItem { Value = "admin", Text = T("Site Admin").Text }),
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

            context.Form("ActionEmail", form);
        }
    }

    public interface IFormHandler : IEventHandler {
        void Validating(dynamic context);
    }

    public class MailFormsValitator : IFormHandler {
        public Localizer T { get; set; }

        public void Validating(dynamic context) {
            if (context.FormName == "ActionEmail") {
                if (context.ValueProdiver.GetValue("Recipient").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Recipient", T("You must select at least one recipient").Text);
                }

                if (context.ValueProdiver.GetValue("Subject").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Subject", T("You must provide a Subject").Text);
                }

                if (context.ValueProdiver.GetValue("Body").AttemptedValue == String.Empty) {
                    context.ModelState.AddModelError("Body", T("You must provide a Body").Text);
                }
            }
        }
    }
}