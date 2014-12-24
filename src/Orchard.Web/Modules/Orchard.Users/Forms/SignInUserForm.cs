using System;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;

namespace Orchard.Users.Forms {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class SignInUserForm : Component, IFormProvider, IFormEventHandler {
        void IFormProvider.Describe(DescribeContext context) {
            context.Form("SignInUser", factory => {
                var shape = (dynamic) factory;
                var form = shape.Form(
                    Id: "signInUser",
                    _UserName: shape.Textbox(
                        Id: "userNameOrEmail",
                        Name: "UserNameOrEmail",
                        Title: T("User Name or Email"),
                        Description: T("The user name or email of the user to sign in."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _Password: shape.Textbox(
                        Id: "password",
                        Name: "Password",
                        Title: T("Password"),
                        Description: T("The password of the user to sign in."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _CreatePersistentCookie: shape.Textbox(
                        Id: "createPersistentCookie",
                        Name: "CreatePersistentCookie",
                        Title: T("Create Persistent Cookie"),
                        Description: T("A value evaluating to 'true' to create a persistent cookie."),
                        Classes: new[]{"text", "large", "tokenized"}));

                return form;
            });
        }

        void IFormEventHandler.Validating(ValidatingContext context) {
            if (context.FormName != "SignInUser") return;

            var userName = context.ValueProvider.GetValue("UserNameOrEmail").AttemptedValue;
            var password = context.ValueProvider.GetValue("Password").AttemptedValue;

            if (String.IsNullOrWhiteSpace(userName)) {
                context.ModelState.AddModelError("UserNameOrEmail", T("You must specify a user name, email address or a token that evaluates to a username or email address.").Text);
            }

            if (String.IsNullOrWhiteSpace(password)) {
                context.ModelState.AddModelError("Password", T("You must specify a password or a token that evaluates to a password.").Text);
            }
        }

        void IFormEventHandler.Building(BuildingContext context) {}
        void IFormEventHandler.Built(BuildingContext context) {}
        void IFormEventHandler.Validated(ValidatingContext context) {}
    }
}