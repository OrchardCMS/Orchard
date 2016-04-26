using System.Collections.Generic;
using System.Web;
using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using System.Linq;
using DescribeContext = Orchard.Forms.Services.DescribeContext;
using System.Web.Mvc;
using Orchard.ContentManagement;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Models;
using System.Collections.Specialized;
using Orchard.DynamicForms.Helpers;
using Orchard.Conditions.Services;

namespace Orchard.DynamicForms.Drivers {
    public class CommandLinkElementDriver : FormsElementDriver<CommandLink> {
        private readonly ITokenizer _tokenizer;
        private readonly ICurrentControllerAccessor _currentControllerAccessor;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IFormService _formService;
        public CommandLinkElementDriver(IFormsBasedElementServices formsServices
            , IConditionManager conditionManager
            , ITokenizer tokenizer
            , ICurrentControllerAccessor currentControllerAccessor
            , IContentManager contentManager
            , IContentDefinitionManager contentDefinitionManager
            , IFormService formService) : base(formsServices, conditionManager) {
            _tokenizer = tokenizer;
            _currentControllerAccessor = currentControllerAccessor;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _formService = formService;
        }

        protected override EditorResult OnBuildEditor(CommandLink element, ElementEditorContext context) {
            var enumerationEditor = BuildForm(context, "CommandLink");
            
            return Editor(context, enumerationEditor);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("CommandLink", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "CommandLink",
                    _DynamicFormCommand: shape.SelectList(
                        Id: "DynamicFormCommand",
                        Name: "DynamicFormCommand",
                        Title: "DynamicFormCommand",
                        Description: T("Select the command to apply."))
                    );

                // Query
                form._DynamicFormCommand.Items.Add(new SelectListItem { Text = DynamicFormCommand.New.ToString(), Value = ((int)DynamicFormCommand.New).ToString() });
                form._DynamicFormCommand.Items.Add(new SelectListItem { Text = DynamicFormCommand.Delete.ToString(), Value = ((int)DynamicFormCommand.Delete).ToString() });
                form._DynamicFormCommand.Items.Add(new SelectListItem { Text = DynamicFormCommand.First.ToString(), Value = ((int)DynamicFormCommand.First).ToString() });
                form._DynamicFormCommand.Items.Add(new SelectListItem { Text = DynamicFormCommand.Previous.ToString(), Value = ((int)DynamicFormCommand.Previous).ToString() });
                form._DynamicFormCommand.Items.Add(new SelectListItem { Text = DynamicFormCommand.Next.ToString(), Value = ((int)DynamicFormCommand.Next).ToString() });
                form._DynamicFormCommand.Items.Add(new SelectListItem { Text = DynamicFormCommand.Last.ToString(), Value = ((int)DynamicFormCommand.Last).ToString() });

                return form;
            });            
        }

        protected override void OnDisplaying(CommandLink element, ElementDisplayingContext context) {
            var controller = _currentControllerAccessor.CurrentController;
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.Enabled = false;
            context.ElementShape.ProcessedValue = "";
            context.ElementShape.DynamicFormCommand = element.DynamicFormCommand.ToString();
            
            if (element.Form == null)
                return;

            int contentIdToEdit = 0;
            int.TryParse(controller.Request.QueryString[HttpUtility.UrlEncode(element.Form.Name + "Form_edit")], out contentIdToEdit);
            if (contentIdToEdit == 0)
                return;

            int contentIdToNavigate = 0;
            if (!_formService.TryGetNextContentIdAfterApplyDynamicFormCommand(context.Content.As<LayoutPart>(), element.Form, element.DynamicFormCommand.ToString(), _contentManager.Get(contentIdToEdit), out contentIdToNavigate))
                return;
            var query = new NameValueCollection();
            query.Add("layoutContentId", context.Content.Id.ToString());
            query.Add("formName", element.Form.Name);
            query.Add("command", element.DynamicFormCommand.ToString());
            query.Add("currentContentId", contentIdToEdit.ToString());
            context.ElementShape.Enabled = true;
            context.ElementShape.ProcessedValue = "Orchard.DynamicForms/Form/Command?" + query.ToQueryString();
        }
    }
}