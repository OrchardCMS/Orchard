using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;

namespace Orchard.Layouts.Drivers {
    public class ElementWrapperPartDriver : ContentPartDriver<ElementWrapperPart> {
        private readonly IElementManager _elementManager;
        private readonly IElementDisplay _elementDisplay;

        public ElementWrapperPartDriver(IElementManager elementManager, IElementDisplay elementDisplay) {
            _elementManager = elementManager;
            _elementDisplay = elementDisplay;
        }

        protected override DriverResult Display(ElementWrapperPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_ElementWrapper", () => {
                var describeContext = CreateDescribeContext(part);
                var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, part.ElementTypeName);
                var data = ElementDataHelper.Deserialize(part.ElementData);
                var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);
                var elementShape = _elementDisplay.DisplayElement(element, part, displayType);
                
                return shapeHelper.Parts_ElementWrapper(ElementShape: elementShape);
            });
        }

        protected override DriverResult Editor(ElementWrapperPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ElementWrapperPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_ElementWrapper_Edit", () => {
                var describeContext = CreateDescribeContext(part);
                var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, part.ElementTypeName);
                var data = ElementDataHelper.Deserialize(part.ElementData);
                var dataClosure = data;
                var element = _elementManager.ActivateElement(descriptor, e => e.Data = dataClosure);
                var context = (ElementEditorContext)CreateEditorContext(describeContext.Content, element, updater, shapeHelper);
                var editorResult = updater != null ? _elementManager.UpdateEditor(context) : _elementManager.BuildEditor(context);
                var viewModel = new ElementWrapperPartViewModel {
                    Tabs = editorResult.CollectTabs().ToArray(),
                    ElementTypeName = part.ElementTypeName,
                    ElementDisplayText = element.DisplayText,
                    ElementEditorResult = editorResult,
                    ElementEditors = editorResult.Editors,
                };

                data = element.Data;

                if (updater != null) {
                    part.ElementData = data.Serialize();
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.ElementWrapper", Model: viewModel, Prefix: Prefix);
            });
        }

        private static DescribeElementsContext CreateDescribeContext(IContent part) {
            return new DescribeElementsContext {
                Content = part
            };
        }

        private ElementEditorContext CreateEditorContext(IContent content, Element element, IUpdateModel updater, dynamic shapeFactory) {
            var context = new ElementEditorContext {
                Content = content,
                Element = element,
                Updater = updater,
                ValueProvider = updater != null ? ((Controller)updater).ValueProvider : null,
                ShapeFactory = shapeFactory,
                Prefix = Prefix
            };
            return context;
        }
    }
}