﻿using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using IController = Orchard.DynamicForms.Services.IController;

namespace Orchard.DynamicForms.Controllers {
    public class FormController : Controller, IController, IUpdateModel {
        private readonly INotifier _notifier;
        private readonly ILayoutManager _layoutManager;
        private readonly IFormService _formService;

        public FormController(
            INotifier notifier, 
            ILayoutManager layoutManager, 
            IFormService formService) {

            _notifier = notifier;
            _layoutManager = layoutManager;
            _formService = formService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost]
        public ActionResult Submit(int contentId, string formName) {
            var layoutPart = _layoutManager.GetLayout(contentId);
            var form = _formService.FindForm(layoutPart, formName);
            var urlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "~/";

            if (form == null) {
                Logger.Warning("The specified form \"{0}\" could not be found.", formName);
                _notifier.Warning(T("The specified form \"{0}\" could not be found."));
                return Redirect(urlReferrer);
            }

            var values = _formService.SubmitForm(form, ValueProvider, ModelState, this);
            this.TransferFormSubmission(form, values);

            if (!ModelState.IsValid)
                return Redirect(urlReferrer);

            if(Response.IsRequestBeingRedirected)
                return new EmptyResult();

            var redirectUrl = !String.IsNullOrWhiteSpace(form.RedirectUrl) ? form.RedirectUrl : urlReferrer;
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