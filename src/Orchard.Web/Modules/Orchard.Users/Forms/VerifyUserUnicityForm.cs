using System;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;

namespace Orchard.Users.Forms {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class VerifyUserUnicityForm : Component, IFormProvider, IFormEventHandler {
        void IFormProvider.Describe(DescribeContext context) {
            context.Form("VerifyUserUnicity", factory => {
                var shape = (dynamic) factory;
                var form = shape.Form(
                    Id: "verifyUserUnicity",
                    _UserName: shape.Textbox(
                        Id: "userName",
                        Name: "UserName",
                        Title: T("User Name"),
                        Description: T("The user name to be validated."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _Email: shape.Textbox(
                        Id: "email",
                        Name: "Email",
                        Title: T("Email"),
                        Description: T("The email address to be validated."),
                        Classes: new[] { "text", "large", "tokenized" }));

                return form;
            });
        }

        void IFormEventHandler.Validating(ValidatingContext context) {
            if (context.FormName != "VerifyUserUnicity") return;

            var userName = context.ValueProvider.GetValue("UserName").AttemptedValue;
            var email = context.ValueProvider.GetValue("Email").AttemptedValue;

            if (String.IsNullOrWhiteSpace(userName)) {
                context.ModelState.AddModelError("UserName", T("You must specify a username or a token that evaluates to a username.").Text);
            }

            if (String.IsNullOrWhiteSpace(email)) {
                context.ModelState.AddModelError("Email", T("You must specify an email address or a token that evaluates to an email address.").Text);
            }
        }

        void IFormEventHandler.Building(BuildingContext context) {}
        void IFormEventHandler.Built(BuildingContext context) {}
        void IFormEventHandler.Validated(ValidatingContext context) {}
    }
}