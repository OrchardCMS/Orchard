using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.CustomForms.Models;
using Orchard.CustomForms.ViewModels;
using Orchard.ContentManagement.Handlers;
using System;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Localization;

namespace Orchard.CustomForms.Drivers {
    public class CustomFormPartDriver : ContentPartDriver<CustomFormPart> {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthorizationService _authService;

        public CustomFormPartDriver(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            IAuthorizationService authService) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _authService = authService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(CustomFormPart part, string displayType, dynamic shapeHelper) {
            // this method is used by the widget to render the form when it is displayed

            var contentItem = _orchardServices.ContentManager.New(part.ContentType);

            if (!contentItem.Has<ICommonPart>()) {
                return null;
            }

            return ContentShape("Parts_CustomForm_Wrapper", () => {
                return shapeHelper.Parts_CustomForm_Wrapper()
                    .Editor(_orchardServices.ContentManager.BuildEditor(contentItem))
                    .ContentPart(part);
            });
        }

        protected override DriverResult Editor(CustomFormPart part, dynamic shapeHelper) {
            return ContentShape("Parts_CustomForm_Fields", () => {
                var contentTypes = _contentDefinitionManager.ListTypeDefinitions().Select(x => x.Name).OrderBy(x => x);
                var viewModel = new CustomFormPartEditViewModel {
                    ContentTypes = contentTypes, 
                    CustomFormPart = part
                };

                return shapeHelper.EditorTemplate(TemplateName: "Parts.CustomForm.Fields", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(CustomFormPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new CustomFormPartEditViewModel {
                CustomFormPart = part
            };

            updater.TryUpdateModel(viewModel, Prefix, null, null);

            // Warn if the custom form is set to save a content item that is viewable by anonymous users (publicly accessible)
            if (viewModel.CustomFormPart.SaveContentItem) {
                // If it's draftable then don't display the warning because the generated content items won't be publicly accessible
                var typeDefinition = _contentDefinitionManager.ListTypeDefinitions().Where(x => String.Equals(x.Name, viewModel.CustomFormPart.ContentType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (typeDefinition != null && !typeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable) {
                    // Create a dummy content item of the specified type to check permissions against
                    if (_authService.TryCheckAccess(Orchard.Core.Contents.Permissions.ViewContent, null, _orchardServices.ContentManager.New(viewModel.CustomFormPart.ContentType))) {
                        _orchardServices.Notifier.Add(UI.Notify.NotifyType.Warning, T("Your custom form will save data to content items that are publicly accessible."));
                    }
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(CustomFormPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "ContentType", x => part.Record.ContentType = x);
            context.ImportAttribute(part.PartDefinition.Name, "SaveContentItem", x => part.Record.SaveContentItem = Boolean.Parse(x));
            context.ImportAttribute(part.PartDefinition.Name, "CustomMessage", x => part.Record.CustomMessage = Boolean.Parse(x));
            context.ImportAttribute(part.PartDefinition.Name, "Message", x => part.Record.Message = x);
            context.ImportAttribute(part.PartDefinition.Name, "Redirect", x => part.Record.Redirect = Boolean.Parse(x));
            context.ImportAttribute(part.PartDefinition.Name, "RedirectUrl", x => part.Record.RedirectUrl = x);
            context.ImportAttribute(part.PartDefinition.Name, "SubmitButtonText", x => part.Record.SubmitButtonText = x);
        }
        
        protected override void Exporting(CustomFormPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("ContentType", part.Record.ContentType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SaveContentItem", part.Record.SaveContentItem);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomMessage", part.Record.CustomMessage);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Message", part.Record.Message);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Redirect", part.Record.Redirect);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RedirectUrl", part.Record.RedirectUrl);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SubmitButtonText", part.Record.SubmitButtonText);
        }
    }
}