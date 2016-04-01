using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Tokens;
using Orchard.UI.Notify;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class FormElementDriver : FormsElementDriver<Form> {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IFormService _formService;
        private readonly ICurrentControllerAccessor _currentControllerAccessor;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly ITokenizer _tokenizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly INotifier _notifier;


        public FormElementDriver(
            IFormsBasedElementServices formsServices, 
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IFormService formService, 
            ICurrentControllerAccessor currentControllerAccessor, 
            ICultureAccessor cultureAccessor, 
            ITokenizer tokenizer,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            INotifier notifier)

            : base(formsServices) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _formService = formService;
            _currentControllerAccessor = currentControllerAccessor;
            _cultureAccessor = cultureAccessor;
            _tokenizer = tokenizer;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            _notifier = notifier;
        }

        protected override IEnumerable<string> FormNames {
            get { yield return "Form"; }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("Form", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Form",
                    _FormName: shape.Textbox(
                        Id: "FormName",
                        Name: "FormName",
                        Title: "Name",
                        Value: "Untitled",
                        Classes: new[] { "text", "medium" },
                        Description: T("The name of the form.")),
                    _FormAction: shape.Textbox(
                        Id: "FormAction",
                        Name: "FormAction",
                        Title: "Custom Target URL",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The action of the form. Leave blank to have the form submitted to the default form controller.")),
                    _FormMethod: shape.SelectList(
                        Id: "FormMethod",
                        Name: "FormMethod",
                        Title: "Method",
                        Description: T("The method of the form.")),
                    _EnableClientValidation: shape.Checkbox(
                        Id: "EnableClientValidation",
                        Name: "EnableClientValidation",
                        Title: "Enable Client Validation",
                        Value: "true",
                        Description: T("Enables client validation.")),
                    _StoreSubmission: shape.Checkbox(
                        Id: "StoreSubmission",
                        Name: "StoreSubmission",
                        Title: "Store Submission",
                        Value: "true",
                        Description: T("Stores the submitted form into the database.")),
                    _HtmlEncode: shape.Checkbox(
                        Id: "HtmlEncode",
                        Name: "HtmlEncode",
                        Title: "Html Encode",
                        Value: "true",
                        Checked: true,
                        Description: T("Check this option to automatically HTML encode submitted values to prevent code injection.")),
                    _CreateContent: shape.Checkbox(
                        Id: "CreateContent",
                        Name: "CreateContent",
                        Title: "Create/Edit/Delete Content",
                        Value: "true",
                        Description: T("Check this option to create/edit/delete a content item based using the submitted values. You will have to select a Content Type here and bind the form fields to the various parts and fields of the selected Content Type. Edit requests can be done using edit={contentitem id} parammeter in the query string.")),
                    _ContentType: shape.SelectList(
                        Id: "FormBindingContentType",
                        Name: "FormBindingContentType",
                        Title: "Content Type",
                        Description: T("The Content Type to use when storing the submitted form values as a content item. Note that if you change the content type, you will have to update the form field bindings."),
                        EnabledBy: "CreateContent"),
                    _PublicationDraft: shape.Radio(
                        Id: "Publication-Draft",
                        Name: "Publication",
                        Title: "Save As Draft",
                        Value: "Draft",
                        Checked: true,
                        Description: T("Save the created content item as a draft."),
                        EnabledBy: "CreateContent"),
                    _PublicationPublish: shape.Radio(
                        Id: "Publication-Publish",
                        Name: "Publication",
                        Title: "Publish",
                        Value: "Publish",
                        Description: T("Publish the created content item."),
                        EnabledBy: "CreateContent"),
                    _Notification: shape.Textbox(
                        Id: "Notification",
                        Name: "Notification",
                        Title: "Show Notification",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The message to show after the form has been submitted. Leave blank if you don't want to show a message.")),
                    _RedirectUrl: shape.Textbox(
                        Id: "RedirectUrl",
                        Name: "RedirectUrl",
                        Title: "Redirect URL",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The URL to redirect to after the form has been submitted. Leave blank to stay on the same page. tip: you can use a Workflow to control what happens when this form is submitted.")));

                // FormMethod
                form._FormMethod.Items.Add(new SelectListItem { Text = "POST", Value = "POST" });
                form._FormMethod.Items.Add(new SelectListItem { Text = "GET", Value = "GET" });

                // ContentType
                var contentTypes = _contentDefinitionManager.ListTypeDefinitions().Where(IsFormBindingContentType).ToArray();
                foreach (var contentType in contentTypes.OrderBy(x => x.DisplayName)) {
                    form._ContentType.Items.Add(new SelectListItem { Text = contentType.DisplayName, Value = contentType.Name });
                }

                return form;
            });
        }

        protected override void OnDisplaying(Form element, ElementDisplayingContext context) {
            var controller = _currentControllerAccessor.CurrentController;
            bool onlyOwnContent = false;
            int contentIdToEdit=0;
            var currentUser = _authenticationService.GetAuthenticatedUser();
            ContentItem contentItemToEdit = null;

            // If form is binded to a content type
            if (element.CreateContent == true && !String.IsNullOrWhiteSpace(element.FormBindingContentType)) {
                string contentIdToEditParam = controller.Request.QueryString[HttpUtility.UrlEncode(element.Name + "Form_edit")];              
                int.TryParse(contentIdToEditParam, out contentIdToEdit);

                // If editing a content item
                if (contentIdToEdit > 0) {
                    context.ElementShape.ContentIdToEdit = contentIdToEdit;                    
                    if (!_authorizationService.TryCheckAccess(Orchard.DynamicForms.Permissions.SubmitAnyFormForModifyData, currentUser, context.Content, element.Name)
                        &&
                        !(onlyOwnContent =_authorizationService.TryCheckAccess(Orchard.DynamicForms.Permissions.SubmitAnyFormForModifyOwnData, currentUser, context.Content, element.Name))) {
                        Logger.Warning("The form \"{0}\" cannot be loaded due to edition permissions", element.Name);
                        return;
                    }

                    ContentTypeDefinition contentTypeDefinition = null;
                    if ((contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(element.FormBindingContentType)) != null
                        && contentTypeDefinition.Name == element.FormBindingContentType) {

                        //Load elements with same version this form can create
                        var versionOptions = VersionOptions.Latest;
                        if (element.Publication == "Publish" || !contentTypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                            versionOptions = VersionOptions.Published;                        
                        contentItemToEdit = _contentManager.Get(contentIdToEdit, versionOptions);
                        if (onlyOwnContent 
                            && !(element.FormBindingContentType == "User" && currentUser.Id == contentItemToEdit.Id) 
                            && (element.FormBindingContentType != "User" && contentItemToEdit.As<CommonPart>().Owner.Id != currentUser.Id)) {
                            Logger.Warning("The form \"{0}\" cannot be loaded due to edition permissions", element.Name);
                            return;
                        }
                    }

                    if (contentItemToEdit == null ) {
                        _notifier.Warning(T("The form \"{0}\" cannot load content item with id \"{1}\"", element.Name, contentIdToEdit));
                        Logger.Warning(String.Format("Attempting to display contem item \"{0}\" that doesn't exist or doesn't match with the specified type in the form \"{1}\".", contentIdToEdit, element.Name));
                        return;
                    }
                }
            }
            
            var modelState = controller != null ? controller.FetchModelState(element) : default(ModelStateDictionary);
            
            if ((modelState != null && !modelState.IsValid) || contentIdToEdit > 0) {
                NameValueCollection values = null;
                // Read any posted values from the previous request.
                if (modelState != null && !modelState.IsValid)
                    values = controller.FetchPostedValues(element);
                // Read values from the content item.
                else
                    values = _formService.GetValuesFromContentItem(contentItemToEdit, element);                                    
                _formService.ReadElementValues(element, new NameValueCollectionValueProvider(values, _cultureAccessor.CurrentCulture));

                // Add any model validation errors from the previous request.
                controller.ApplyAnyModelErrors(element, modelState);
            }
            
            // Assign the binding content type to each element within the form element.
            foreach (var child in element.Elements.Flatten().Where(x => x is FormElement).Cast<FormElement>()) {
                child.FormBindingContentType = element.CreateContent == true ? element.FormBindingContentType : default(string);
            }
            
            // Set tokenized properties.
            var tokenData = context.GetTokenData();
            context.ElementShape.ProcessedAction = _tokenizer.Replace(element.Action, tokenData);
        }
        
        private static bool IsFormBindingContentType(ContentTypeDefinition contentTypeDefinition) {
            var blacklist = new[] {"Site", "Layer"};

            return !blacklist.Any(x => contentTypeDefinition.Name == x) && String.IsNullOrEmpty(contentTypeDefinition.Stereotype());
        }
    }
}