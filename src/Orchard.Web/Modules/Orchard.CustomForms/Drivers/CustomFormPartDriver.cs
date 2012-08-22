using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.CustomForms.Models;
using Orchard.CustomForms.ViewModels;

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
                    .ContenItem(part)
                    .ReturnUrl(part.Redirect ? part.RedirectUrl : _orchardServices.WorkContext.HttpContext.Request.RawUrl);
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
    }
}