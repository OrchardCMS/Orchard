using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Services;

namespace Orchard.AntiSpam.Drivers {
    public class ReCaptchaPartDriver : ContentPartDriver<ReCaptchaPart> {
        private readonly INotifier _notifier;
        private readonly IJsonConverter _jsonConverter;
        private readonly IWorkContextAccessor _workContextAccessor;
        private const string ReCaptchaSecureUrl = "https://www.google.com/recaptcha/api/siteverify";

        public ReCaptchaPartDriver(
            INotifier notifier,
            IJsonConverter jsonConverter,
            IWorkContextAccessor workContextAccessor) {
            _notifier = notifier;
            _jsonConverter = jsonConverter;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        protected override DriverResult Editor(ReCaptchaPart part, dynamic shapeHelper) {
            var workContext = _workContextAccessor.GetContext();

            // don't display the part in the admin
            if (AdminFilter.IsApplied(workContext.HttpContext.Request.RequestContext)) {
                return null;
            }

            return ContentShape("Parts_ReCaptcha_Fields", () => {
                var settings = workContext.CurrentSite.As<ReCaptchaSettingsPart>();

                if (settings.TrustAuthenticatedUsers && workContext.CurrentUser != null) {
                    return null;
                }

                var viewModel = new ReCaptchaPartEditViewModel {
                    PublicKey = settings.PublicKey
                };

                return shapeHelper.EditorTemplate(TemplateName: "Parts.ReCaptcha.Fields", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(ReCaptchaPart part, IUpdateModel updater, dynamic shapeHelper) {
            var workContext = _workContextAccessor.GetContext();
            var settings = workContext.CurrentSite.As<ReCaptchaSettingsPart>();

            // don't display the part in the admin
            if (AdminFilter.IsApplied(workContext.HttpContext.Request.RequestContext)) {
                return null;
            }

            if (settings.TrustAuthenticatedUsers && workContext.CurrentUser != null) {
                return null;
            }

            var context = workContext.HttpContext;

            try {
                var result = ExecuteValidateRequest(
                    settings.PrivateKey,
                    context.Request.ServerVariables["REMOTE_ADDR"],
                    context.Request.Form["g-recaptcha-response"]
                    );

                ReCaptchaPartResponseModel responseModel = _jsonConverter.Deserialize<ReCaptchaPartResponseModel>(result);

                if (!responseModel.Success) {
                    foreach (var errorCode in responseModel.ErrorCodes) {
                        if(errorCode == "missing-input-response") {
                            updater.AddModelError("", T("Please prove that you are not a bot."));
                            _notifier.Error(T("Please prove that you are not a bot."));
                        }
                        else {
                            Logger.Information("An error occurred while submitting a reCaptcha: " + errorCode);
                            updater.AddModelError("", T("An error occurred while submitting a reCaptcha."));
                            _notifier.Error(T("An error occurred while submitting a reCaptcha."));
                        }
                    }
                }
            }
            catch (Exception e) {
                Logger.Error(e, "An unexcepted error occurred while submitting a reCaptcha.");
                updater.AddModelError("", T("There was an error while validating the Captcha."));
                _notifier.Error(T("There was an error while validating the Captcha."));
            }

            return Editor(part, shapeHelper);
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