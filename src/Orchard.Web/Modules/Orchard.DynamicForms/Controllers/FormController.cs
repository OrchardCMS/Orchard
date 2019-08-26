using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using IController = Orchard.DynamicForms.Services.IController;

namespace Orchard.DynamicForms.Controllers {
    public class FormController : Controller, IController, IUpdateModel {
        private readonly INotifier _notifier;
        private readonly ILayoutManager _layoutManager;
        private readonly IFormService _formService;
        private readonly ITokenizer _tokenizer;
        private readonly IClock _clock;

        public FormController(
            INotifier notifier, 
            ILayoutManager layoutManager, 
            IFormService formService, 
            ITokenizer tokenizer,
            IClock clock) {

            _notifier = notifier;
            _layoutManager = layoutManager;
            _formService = formService;
            _tokenizer = tokenizer;
            _clock = clock;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Submit(int contentId, string formName) {
            var layoutPart = _layoutManager.GetLayout(contentId);
            var form = _formService.FindForm(layoutPart, formName);
            var urlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.LocalPath : "~/";

            if (form == null) {
                Logger.Warning("The specified form \"{0}\" could not be found.", formName);
                _notifier.Warning(T("The specified form \"{0}\" could not be found.", formName));
                return Redirect(urlReferrer);
            }

            var values = _formService.SubmitForm(layoutPart, form, ValueProvider, ModelState, this);
            this.TransferFormSubmission(form, values);

            if (!ModelState.IsValid) {
                // We need a way to inform the output cache filter to not cache the upcoming request.
                var epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;
                var refresh = _clock.UtcNow.Ticks - epoch;
                return Redirect(urlReferrer + "?__r=" + refresh);
            }

            if(Response.IsRequestBeingRedirected)
                return new EmptyResult();

            var redirectUrl = !String.IsNullOrWhiteSpace(form.RedirectUrl) ? _tokenizer.Replace(form.RedirectUrl, new { Content = layoutPart.ContentItem }) : urlReferrer;
            return Redirect(redirectUrl);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}