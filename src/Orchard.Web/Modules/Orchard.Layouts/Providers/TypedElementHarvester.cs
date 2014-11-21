using System.Collections.Generic;
using System.Linq;
using Orchard.Environment;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Providers {
    public class TypedElementHarvester : IElementHarvester {
        private readonly Work<IElementManager> _elementManager;
        private readonly Work<IElementFactory> _factory;

        public TypedElementHarvester(Work<IElementManager> elementManager, Work<IElementFactory> factory) {
            _elementManager = elementManager;
            _factory = factory;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var drivers = _elementManager.Value.GetDrivers();
            var elementTypes = drivers.Select(x => x.GetType().BaseType.GenericTypeArguments[0]).Where(x => !x.IsAbstract && !x.IsInterface).ToArray();
            return elementTypes.Select(elementType => {
                var element = _factory.Value.Activate(elementType);
                return new ElementDescriptor(elementType, element.Type, element.DisplayText, element.Category) {
                    GetDrivers = () => _elementManager.Value.GetDrivers(element),
                    Editor = Editor,
                    UpdateEditor = Editor,
                    IsSystemElement = element.IsSystemElement,
                    EnableEditorDialog = element.HasEditor
                };
            });
        }

        private void Editor(ElementEditorContext context) {
            var viewModel = context.Element.State.GetModel<CommonElementSettings>();
            var commonSettingsEditor = context.ShapeFactory.EditorTemplate(
                TemplateName: "ElementSettings.Common",
                Model: viewModel,
                Prefix: "CommonElementSettings");

            commonSettingsEditor.Metadata.Position = "Settings:5";
            context.EditorResult.Add(commonSettingsEditor);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix.AppendPrefix("CommonElementSettings"), null, null);
                context.Element.State = context.Element.State.Combine(new StateDictionary {
                    {"CommonElementSettings.Id", viewModel.Id},
                    {"CommonElementSettings.CssClass", viewModel.CssClass},
                    {"CommonElementSettings.InlineStyle", viewModel.InlineStyle}
                });
            }
        }
    }
}