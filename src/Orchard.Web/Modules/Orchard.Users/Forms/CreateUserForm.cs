using System;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;

namespace Orchard.Users.Forms {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class CreateUserForm : Component, IFormProvider, IFormEventHandler {

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CreateUserForm(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        void IFormProvider.Describe(DescribeContext context) {
            context.Form("CreateUser", factory => {
                var shape = (dynamic) factory;
                var form = shape.Form(
                    Id: "createUser",
                    _UserName: shape.Textbox(
                        Id: "userName",
                        Name: "UserName",
                        Title: T("User Name"),
                        Description: T("The user name of the user to be created."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _Email: shape.Textbox(
                        Id: "email",
                        Name: "Email",
                        Title: T("Email"),
                        Description: T("The email address of the user to be created."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _Password: shape.Textbox(
                        Id: "password",
                        Name: "Password",
                        Title: T("Password"),
                        Description: T("The password of the user to be created."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _Approved: shape.Checkbox(
                        Id: "approved",
                        Name: "Approved",
                        Title: T("Approved"),
                        Description: T("Check to approve the created user."),
                        Value: true),
                    _ContentType: shape.Textbox(
                        Id: "contentType",
                        Name: "ContentType",
                        Title: T("ContentType"),                        
                        Description: T("The user content type to be created. If none is set Content Type User will be used."),
                        Classes: new[] { "text" }));

                return form;
            });
        }

        void IFormEventHandler.Validating(ValidatingContext context) {
            if (context.FormName != "CreateUser") return;

            var userName = context.ValueProvider.GetValue("UserName").AttemptedValue;
            var email = context.ValueProvider.GetValue("Email").AttemptedValue;
            var password = context.ValueProvider.GetValue("Password").AttemptedValue;
            var contentType = context.ValueProvider.GetValue("ContentType").AttemptedValue;

            if (String.IsNullOrWhiteSpace(userName)) {
                context.ModelState.AddModelError("UserName", T("You must specify a username or a token that evaluates to a username.").Text);
            }

            if (String.IsNullOrWhiteSpace(email)) {
                context.ModelState.AddModelError("Email", T("You must specify an email address or a token that evaluates to an email address.").Text);
            }

            if (String.IsNullOrWhiteSpace(password)) {
                context.ModelState.AddModelError("Password", T("You must specify a password or a token that evaluates to a password.").Text);
            }

            if (!String.IsNullOrWhiteSpace(contentType) && 
                (_contentDefinitionManager.GetTypeDefinition(contentType)==null || 
                    !_contentDefinitionManager.GetTypeDefinition(contentType).Parts.Any(p=>p.PartDefinition.Name=="UserPart"))) {
                context.ModelState.AddModelError("CreateUser", T("You must specify a content type composed by an UserPart.").Text);
            }
        }

        void IFormEventHandler.Building(BuildingContext context) {}
        void IFormEventHandler.Built(BuildingContext context) {}
        void IFormEventHandler.Validated(ValidatingContext context) {}
    }
}