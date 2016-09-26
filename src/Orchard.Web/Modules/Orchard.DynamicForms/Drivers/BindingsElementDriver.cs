using System;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ViewModels;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.DynamicForms.Drivers {
    public class BindingsElementDriver : ElementDriver<FormElement> {
        private readonly IBindingManager _bindingManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public BindingsElementDriver(
            IBindingManager bindingManager, 
            IContentDefinitionManager contentDefinitionManager) {

            _bindingManager = bindingManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override EditorResult OnBuildEditor(FormElement element, ElementEditorContext context) {
            var contentType = element.FormBindingContentType;
            var contentTypeDefinition = !String.IsNullOrWhiteSpace(contentType) ? _contentDefinitionManager.GetTypeDefinition(contentType) : default(ContentTypeDefinition);

            if (contentTypeDefinition == null)
                return null;

            var viewModel = element.Data.GetModel<FormBindingSettings>() ?? new FormBindingSettings();
            viewModel.AvailableBindings = _bindingManager.DescribeBindingsFor(contentTypeDefinition).ToArray();

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, null, null, new[] {"AvailableBindings"});
                viewModel.Store(element.Data);
            }

            var bindingsEditor = context.ShapeFactory.EditorTemplate(TemplateName: "FormBindings", Model: viewModel);

            bindingsEditor.Metadata.Position = "Bindings:10";
            
            return Editor(context, bindingsEditor);
        }
    }
}