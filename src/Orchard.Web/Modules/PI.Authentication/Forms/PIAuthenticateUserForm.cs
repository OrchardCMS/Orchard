using Orchard;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PI.Authentication.Forms
{
    //[OrchardFeature("PIUserAuthentication.Workflows")]
    public class PIAuthenticateUserForm : Component, IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public PIAuthenticateUserForm(IShapeFactory shapeFactory)
        {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        void IFormProvider.Describe(DescribeContext context)
        {
            context.Form("PIAuthenticateUserForm", factory =>
            {
                var shape = (dynamic)factory;
                var form = shape.Form(
                    Id: "authenticateUserForm",
                    _UserName: shape.Textbox(
                        Id: "userName",
                        Name: "UserName",
                        Title: T("User Name"),
                        Description: T("The user name of the user for Go Pack."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _Password: shape.Textbox(
                        Id: "password",
                        Name: "Password",
                        Title: T("Password"),
                        Description: T("The Password of the user for Go Pack"),
                        Classes: new[] { "text", "large", "tokenized" }),
                _RememberMe: shape.Textbox(
                    Id: "rememberMe",
                    Name: "RememberMe",
                    Title: T("Remember Me"),
                     Description: T("Remember Me value"),
                    Classes: new[] { "text", "large", "tokenized" }));
                return form;
            });
        }
    }
}