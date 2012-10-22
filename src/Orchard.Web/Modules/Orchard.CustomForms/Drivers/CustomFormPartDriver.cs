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

namespace Orchard.CustomForms.Drivers {
    public class CustomFormPartDriver : ContentPartDriver<CustomFormPart> {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;

        public CustomFormPartDriver(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
        }

        protected override DriverResult Display(CustomFormPart part, string displayType, dynamic shapeHelper) {
            // this method is used by the widget to render the form when it is displayed

            var contentItem = _orchardServices.ContentManager.New(part.ContentType);

            if (!contentItem.Has<ICommonPart>()) {
                return null;
            }

            return ContentShape("Parts_CustomForm_Wrapper", () => {
                return shapeHelper.Parts_CustomForm_Wrapper()
                    .Editor(_orchardServices.ContentManager.BuildEditor(contentItem))
                    .ContenItem(part);
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
            return Editor(part, shapeHelper);
        }

        protected override void Importing(CustomFormPart part, ImportContentContext context) {
            IfNotNull(context.Attribute(part.PartDefinition.Name, "ContentType"), x => part.Record.ContentType = x);
            IfNotNull(context.Attribute(part.PartDefinition.Name, "SaveContentItem"), x => part.Record.SaveContentItem = Boolean.Parse(x));
            IfNotNull(context.Attribute(part.PartDefinition.Name, "CustomMessage"), x => part.Record.CustomMessage = Boolean.Parse(x));
            IfNotNull(context.Attribute(part.PartDefinition.Name, "Message"), x => part.Record.Message = x);
            IfNotNull(context.Attribute(part.PartDefinition.Name, "Redirect"), x => part.Record.Redirect = Boolean.Parse(x));
            IfNotNull(context.Attribute(part.PartDefinition.Name, "RedirectUrl"), x => part.Record.RedirectUrl = x);
        }

        private static void IfNotNull<T>(T value, Action<T> then) {
            if (value != null) {
                then(value);
            }
        }

        protected override void Exporting(CustomFormPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("ContentType", part.Record.ContentType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SaveContentItem", part.Record.SaveContentItem);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomMessage", part.Record.CustomMessage);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Message", part.Record.Message);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Redirect", part.Record.Redirect);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RedirectUrl", part.Record.RedirectUrl);
        }
    }
}