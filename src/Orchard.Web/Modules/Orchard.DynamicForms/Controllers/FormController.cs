using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Services;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using IController = Orchard.DynamicForms.Services.IController;
using Orchard.Mvc.Html;
using System.Web;
using System.Linq;
using System.Collections.Specialized;
using Orchard.Core.Contents.Settings;
using Orchard.ContentManagement.MetaData;

namespace Orchard.DynamicForms.Controllers {
    public class FormController : Controller, IController, IUpdateModel {
        private readonly INotifier _notifier;
        private readonly ILayoutManager _layoutManager;
        private readonly IFormService _formService;
        private readonly ITokenizer _tokenizer;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public FormController(
            INotifier notifier, 
            ILayoutManager layoutManager, 
            IFormService formService, 
            ITokenizer tokenizer,
            IClock clock,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            IContentDefinitionManager contentDefinitionManager) {

            _notifier = notifier;
            _layoutManager = layoutManager;
            _formService = formService;
            _tokenizer = tokenizer;
            _clock = clock;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost, ActionName("Submit")]
        [ValidateInput(false)]
        public ActionResult Submit(int contentId, string formName) {            
            var layoutPart = _layoutManager.GetLayout(contentId);
            var form = _formService.FindForm(layoutPart, formName);
            var user = _authenticationService.GetAuthenticatedUser();
            bool onlyOwnContent = false;
            var urlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : "~/";                        

            if (form == null) {
                Logger.Warning("The specified form \"{0}\" could not be found.", formName);
                _notifier.Warning(T("The specified form \"{0}\" could not be found.", formName));
                return Redirect(urlReferrer);
            }

            int contenItemIdToEdit=0;
            var permission = Permissions.SubmitAnyForm;
            if (form.CreateContent == true && !String.IsNullOrWhiteSpace(form.FormBindingContentType)) {                
                if (int.TryParse(Request.Form.Get("contentIdToEdit"), out contenItemIdToEdit) && contenItemIdToEdit > 0)
                    permission = Permissions.SubmitAnyFormForModifyData;                
            }
            if (!_authorizationService.TryCheckAccess(permission, user, layoutPart.ContentItem, formName)
                &&
                Permissions.GetOwnerVariation(permission)!= null 
                &&
                !(onlyOwnContent = _authorizationService.TryCheckAccess(Permissions.GetOwnerVariation(permission), user, layoutPart.ContentItem, formName))
                ) {
                Logger.Warning("Insufficient permissions for submitting the specified form \"{0}\".", formName);
                _notifier.Warning(T("Insufficient permissions for submitting the specified form \"{0}\".",formName));
                return Redirect(urlReferrer);
            }

            if (contenItemIdToEdit > 0) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(form.FormBindingContentType);
                var versionOptions = VersionOptions.Latest;
                if (form.Publication == "Publish" || !contentTypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    versionOptions = VersionOptions.Published;

                var contentItemToEdit = _contentManager.Get(contenItemIdToEdit, versionOptions);
                var isAUserType = contentTypeDefinition.Parts.Any(p => p.PartDefinition.Name == "UserPart");
                if (onlyOwnContent
                    && !(isAUserType && user.Id == contentItemToEdit.Id)
                    && (!isAUserType && contentItemToEdit.As<CommonPart>().Owner.Id != user.Id)) {
                    Logger.Warning("The form \"{0}\" cannot be loaded due to edition permissions", form.Name);
                    _notifier.Warning(T("Insufficient permissions for submitting the specified form \"{0}\".", formName));
                    return Redirect(urlReferrer);
                }
            }

            var values = _formService.SubmitForm(layoutPart, form, ValueProvider, ModelState, this, contenItemIdToEdit);
            this.TransferFormSubmission(form, values);

            if (!ModelState.IsValid) {
                // We need a way to inform the output cache filter to not cache the upcoming request.
                var epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;
                var refresh = _clock.UtcNow.Ticks - epoch;
                return Redirect(urlReferrer + "?__r=" + refresh);
            }

            if(Response.IsRequestBeingRedirected)
                return new EmptyResult();


            if (!String.IsNullOrWhiteSpace(form.RedirectUrl)) 
                return Redirect(_tokenizer.Replace(form.RedirectUrl, new { Content = layoutPart.ContentItem }));
            
            //I don't know how to get elegantly the id fo a contentitem added to navigate to it if permissions allow it;
            if (Request.UrlReferrer != null && _authorizationService.TryCheckAccess(Permissions.SubmitAnyFormForModifyOwnData, user, layoutPart.ContentItem, form.Name)) {
                var query = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
                query[HttpUtility.UrlEncode(formName + "Form_edit")] = contenItemIdToEdit.ToString();
                return Redirect(Request.UrlReferrer.LocalPath + "?" + query.ToQueryString());
            }
            return Redirect(urlReferrer);
        }

        [HttpGet]
        public ActionResult Command(int layoutContentId, string formName, string command, int currentContentId=0) {
            var layoutPart = _layoutManager.GetLayout(layoutContentId);
            var form = _formService.FindForm(layoutPart, formName);
            
            var urlReferrer = ((Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "~/");

            if (form == null) {
                Logger.Warning("The specified form \"{0}\" could not be found.", formName);
                _notifier.Warning(T("The specified form \"{0}\" could not be found.", formName));
                return Redirect(urlReferrer);
            }
            
            if (form.CreateContent != true || String.IsNullOrWhiteSpace(form.FormBindingContentType)) {
                _notifier.Warning(T("The form \"{0}\" cannot load invalid data", formName));
                Logger.Warning(String.Format("Attempting to display contem item s in form \"{1}\" not binded to a content type.", formName));
                return Redirect(urlReferrer);
            }            

            int contentItemId = 0;
            var contentItem = _contentManager.Get(currentContentId);
            if (!_formService.TryGetNextContentIdAfterApplyDynamicFormCommand(layoutPart, form, command, contentItem, out contentItemId))
                return Redirect(urlReferrer);

            if (string.Compare(DynamicFormCommand.Delete.ToString(),command)==0)                
                _contentManager.Remove(contentItem);

            var query = new NameValueCollection();
            var contentLocalPath = (new UrlHelper(Request.RequestContext)).ItemDisplayUrl(layoutPart.ContentItem);
            if (Request.UrlReferrer.LocalPath == contentLocalPath) {
                query = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
                query[HttpUtility.UrlEncode(formName + "Form_edit")] = contentItemId.ToString();
                urlReferrer = contentLocalPath + "?" + query.ToQueryString();
            }
            return Redirect(urlReferrer);            
            
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}