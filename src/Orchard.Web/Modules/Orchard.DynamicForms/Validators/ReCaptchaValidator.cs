using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.ViewModels;
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

        private const string ReCaptchaSecureUrl = "https://www.google.com/recaptcha/api/siteverify";

        protected override void OnValidate(ReCaptcha element, ValidateInputContext context) {
            var workContext = _workContextAccessor.GetContext();
            var settings = workContext.CurrentSite.As<ReCaptchaSettingsPart>();

            if (settings.TrustAuthenticatedUsers && workContext.CurrentUser != null) {
                return;
            }

            var httpContext = workContext.HttpContext;
            var response = context.Values["g-recaptcha-response"];

            if (context.ModelState.IsValid) {
                try {
                    var result = ExecuteValidateRequest(
                        settings.PrivateKey,
                        httpContext.Request.ServerVariables["REMOTE_ADDR"],
                        response
                        );

                    ReCaptchaElementResponseModel responseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptchaElementResponseModel>(result);

                    if (!responseModel.Success) {
                        for (int i = 0; i < responseModel.ErrorCodes.Length; i++) {
                            if (responseModel.ErrorCodes[i] == "missing-input-response") {
                                var validationSettings = element.ValidationSettings;
                                var validationMessage = validationSettings.CustomValidationMessage.WithDefault("The Captcha field is required");
                                context.ModelState.AddModelError("g-recaptcha-response", T(validationMessage).Text);
                            }
                            else {
                                var validationSettings = element.ValidationSettings;
                                var validationMessage = validationSettings.CustomValidationMessage.WithDefault("There was an error with the Captcha please try again");
                                context.ModelState.AddModelError("g-recaptcha-response", T(validationMessage).Text);
                                Logger.Information("Error occurred while submitting a reCaptcha: " + responseModel.ErrorCodes[i]);
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Logger.Error(e, "An unexcepted error occurred while submitting a reCaptcha");
                    context.ModelState.AddModelError("recaptcha_response_field", T("There was an error while validating the Captcha image.").Text);
                }
            }
        }

        private static string ExecuteValidateRequest(string privateKey, string remoteip, string response) {
            var postData = String.Format(CultureInfo.InvariantCulture,
                "secret={0}&response={1}&remoteip={2}",
                privateKey,
                response,
                remoteip
            );

            WebRequest request = WebRequest.Create(ReCaptchaSecureUrl + "?" + postData);
            request.Method = "GET";
            request.Timeout = 5000; //milliseconds
            request.ContentType = "application/x-www-form-urlencoded";

            using (WebResponse webResponse = request.GetResponse()) {
                using (var reader = new StreamReader(webResponse.GetResponseStream())) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}