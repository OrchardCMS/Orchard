using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Settings;
using Orchard.AntiSpam.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.AntiSpam.Drivers {
    public class ReCaptchaPartDriver : ContentPartDriver<ReCaptchaPart> {
        private readonly IWorkContextAccessor _workContextAccessor;
        private const string ReCaptchaUrl = "http://www.google.com/recaptcha/api";
        private const string ReCaptchaSecureUrl = "https://www.google.com/recaptcha/api";
        
        public ReCaptchaPartDriver(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(ReCaptchaPart part, dynamic shapeHelper) {
            return ContentShape("Parts_ReCaptcha_Fields", () => {
                var settings = part.TypePartDefinition.Settings.GetModel<ReCaptchaPartSettings>();
                var viewModel = new ReCaptchaPartEditViewModel {
                    PublicKey =  settings.PublicKey
                };

                return shapeHelper.EditorTemplate(TemplateName: "Parts.ReCaptcha.Fields", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(ReCaptchaPart part, IUpdateModel updater, dynamic shapeHelper) {

            var submitViewModel = new ReCaptchaPartSubmitViewModel();

            if(updater.TryUpdateModel(submitViewModel, String.Empty, null, null)) {
                var settings = part.TypePartDefinition.Settings.GetModel<ReCaptchaPartSettings>();
                var context = _workContextAccessor.GetContext().HttpContext;

                var result = ExecuteValidateRequest(
                    settings.PrivateKey, 
                    context.Request.ServerVariables["REMOTE_ADDR"], 
                    submitViewModel.recaptcha_challenge_field, 
                    submitViewModel.recaptcha_response_field
                    );

                if(!HandleValidateResponse(context, result)) {
                    updater.AddModelError("Parts_ReCaptcha_Fields", T("Incorrect word"));
                }
            }


            return Editor(part, shapeHelper);
        }

        private static string ExecuteValidateRequest(string privateKey, string remoteip, string challenge, string response) {
            WebRequest request = WebRequest.Create(ReCaptchaUrl + "/verify");
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

            byte[] content = Encoding.UTF8.GetBytes(postData);
            using (Stream stream = request.GetRequestStream()) {
                stream.Write(content, 0, content.Length);
            }
            using (WebResponse webResponse = request.GetResponse()) {
                using (var reader = new StreamReader(webResponse.GetResponseStream())) {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static bool HandleValidateResponse(HttpContextBase context, string response) {
            if (!String.IsNullOrEmpty(response)) {
                string[] results = response.Split('\n');
                if (results.Length > 0) {
                    bool rval = Convert.ToBoolean(results[0], CultureInfo.InvariantCulture);
                    return rval;
                }
            }
            return false;
        }
    }
}