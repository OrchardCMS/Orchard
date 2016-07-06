using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.CustomForms.Activities;
using Orchard.CustomForms.Models;
using Orchard.CustomForms.Rules;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;

namespace Orchard.CustomForms.Controllers {
    [Themed(true)]
    [ValidateInput(false)]
    public class ItemController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IRulesManager _rulesManager;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkflowManager _workflowManager;

        public ItemController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            ITransactionManager transactionManager,
            IShapeFactory shapeFactory,
            IRulesManager rulesManager,
            ITokenizer tokenizer,
            IWorkflowManager workflowManager) {
            Services = orchardServices;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _rulesManager = rulesManager;
            _tokenizer = tokenizer;
            _workflowManager = workflowManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Create(int id) {
            var form = _contentManager.Get(id);

            if(form == null || !form.Has<CustomFormPart>()) {
                return HttpNotFound();
            }

            var customForm = form.As<CustomFormPart>();

            var contentItem = _contentManager.New(customForm.ContentType);

            if(!contentItem.Has<ICommonPart>()) {
                throw new OrchardException(T("The content type must have CommonPart attached"));
            }

            if (!Services.Authorizer.Authorize(Permissions.CreateSubmitPermission(customForm.ContentType), contentItem, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            var model = _contentManager.BuildEditor(contentItem);

            model
                .ContentItem(form)
                .ReturnUrl(Url.RouteUrl(_contentManager.GetItemMetadata(form).DisplayRouteValues));

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(int id, string returnUrl) {
            return CreatePOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST(int id, string returnUrl) {
            var form = _contentManager.Get(id);

            if (form == null || !form.Has<CustomFormPart>()) {
                return HttpNotFound();
            }

            var customForm = form.As<CustomFormPart>();

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(customForm.ContentType);

            if (!Services.Authorizer.Authorize(Permissions.CreateSubmitPermission(customForm.ContentType), dummyContent, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            return CreatePOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult CreatePOST(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var form = _contentManager.Get(id);

            if (form == null || !form.Has<CustomFormPart>()) {
                return HttpNotFound();
            }

            var customForm = form.As<CustomFormPart>();
            var contentItem = _contentManager.New(customForm.ContentType);

            if (!Services.Authorizer.Authorize(Permissions.CreateSubmitPermission(customForm.ContentType), contentItem, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            if(customForm.SaveContentItem)
                _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();

                // if custom form is inside a widget, we display the form itself
                if (form.ContentType == "CustomFormWidget") {
                    foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage)) {
                        Services.Notifier.Error(new LocalizedString(HttpUtility.HtmlEncode(error)));
                    }

                    // save the updated editor shape into TempData to survive a redirection and keep the edited values
                    TempData["CustomFormWidget.InvalidCustomFormState"] = model;

                    if (returnUrl != null) {
                        return this.RedirectLocal(returnUrl);
                    }
                }

                model.ContentItem(form);
                return View(model);
            }

            contentItem.As<ICommonPart>().Container = customForm.ContentItem;

            // triggers any event
            _rulesManager.TriggerEvent("CustomForm", "Submitted",
                    () => new Dictionary<string, object> { { "Content", contentItem } });

            // trigger any workflow
            _workflowManager.TriggerEvent(FormSubmittedActivity.EventName, contentItem,
                    () => new Dictionary<string, object> { { "Content", contentItem} , { "CustomForm", customForm.ContentItem } });

            if (customForm.Redirect) {
                returnUrl = _tokenizer.Replace(customForm.RedirectUrl, new Dictionary<string, object> { { "Content", contentItem } });
            }

            // save the submitted form
            if (customForm.SaveContentItem) {
                conditionallyPublish(contentItem);
            }

            // writes a confirmation message
            if (customForm.CustomMessage) {
                if (!String.IsNullOrWhiteSpace(customForm.Message)) {
                    Services.Notifier.Success(T(customForm.Message));
                }
            }

            var referrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : null;
            return this.RedirectLocal(returnUrl, () => this.RedirectLocal(referrer, () => Redirect(Request.RawUrl)));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
