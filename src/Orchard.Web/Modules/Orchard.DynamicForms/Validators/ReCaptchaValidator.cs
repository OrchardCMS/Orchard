using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Logging;

namespace Orchard.DynamicForms.Validators {
    public class ReCaptchaValidator : ElementValidator<ReCaptcha> {
        private readonly IWorkContextAccessor _workContextAccessor;
        public ReCaptchaValidator(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        private const string ReCaptchaUrl = "http://www.google.com/recaptcha/api";

        protected override void OnValidate(ReCaptcha element, ValidateInputContext context) {
            var workContext = _workContextAccessor.GetContext();
            var settings = workContext.CurrentSite.As<ReCaptchaSettingsPart>();

            if (settings.TrustAuthenticatedUsers && workContext.CurrentUser != null) {
                return;
            }

            var httpContext = workContext.HttpContext;
            var response = context.Values["recaptcha_response_field"];
            var challenge = context.Values["recaptcha_challenge_field"];

            if (context.ModelState.IsValid) {
                try {
                    var result = ExecuteValidateRequest(
                        settings.PrivateKey,
                        httpContext.Request.ServerVariables["REMOTE_ADDR"],
                        challenge,
                        response
                        );

                    if (!HandleValidateResponse(httpContext, result)) {
                        var validationSettings = element.ValidationSettings;
                        var validationMessage = validationSettings.CustomValidationMessage.WithDefault("The text you entered in the Captcha field does not match the image. Please try again.");
                        context.ModelState.AddModelError("recaptcha_response_field", T(validationMessage).Text);
                    }
                }
                catch (Exception e) {
                    Logger.Error(e, "An unexcepted error occured while submitting a reCaptcha");
                    context.ModelState.AddModelError("recaptcha_response_field", T("There was an error while validating the Captcha image.").Text);
                }
            }
        }

        private static string ExecuteValidateRequest(string privateKey, string remoteip, string challenge, string response) {
            var request = WebRequest.Create(ReCaptchaUrl + "/verify");
            request.Method = "POST";
            request.Timeout = 5000; //milliseconds
            request.ContentType = "application/x-www-form-urlencoded";

            var postData = String.Format(CultureInfo.InvariantCulture,
                "privatekey={0}&remoteip={1}&challenge={2}&response={3}",
                privateKey,
                remoteip,
                challenge,
                response
            );

            var content = Encoding.UTF8.GetBytes(postData);
            using (var stream = request.GetRequestStream()) {
                stream.Write(content, 0, content.Length);
            }
            using (var webResponse = request.GetResponse()) {
                using (var reader = new StreamReader(webResponse.GetResponseStream())) {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static bool HandleValidateResponse(HttpContextBase context, string response) {
            if (!String.IsNullOrEmpty(response)) {
                var results = response.Split('\n');
                if (results.Length > 0) {
                    var rval = Convert.ToBoolean(results[0], CultureInfo.InvariantCulture);
                    return rval;
                }
            }
            return false;
        }
    }
}