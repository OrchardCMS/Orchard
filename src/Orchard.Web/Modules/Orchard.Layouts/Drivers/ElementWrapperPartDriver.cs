using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
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
        private readonly IElementSerializer _serializer;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly IWorkContextAccessor _wca;

        public ElementWrapperPartDriver(
            IElementManager elementManager, 
            IElementDisplay elementDisplay, 
            IElementSerializer serializer, 
            ICultureAccessor cultureAccessor, 
            IWorkContextAccessor wca) {

            _elementManager = elementManager;
            _elementDisplay = elementDisplay;
            _serializer = serializer;
            _cultureAccessor = cultureAccessor;
            _wca = wca;
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
                var data = ElementDataHelper.Deserialize(part.ElementData).Combine(_wca.GetContext().HttpContext.Request.Form.ToDictionary());
                var dataClosure = data;
                var element = _elementManager.ActivateElement(descriptor, e => e.Data = dataClosure);
                var context = CreateEditorContext(describeContext.Content, element, data, updater, shapeHelper);
                var editorResult = (EditorResult)(updater != null ? _elementManager.UpdateEditor(context) : _elementManager.BuildEditor(context));
                var viewModel = new ElementWrapperPartViewModel {
                    Tabs = editorResult.CollectTabs().ToArray(),
                    ElementTypeName = part.ElementTypeName,
                    ElementDisplayText = element.DisplayText,
                    ElementEditorResult = editorResult,
                    ElementEditors = editorResult.Editors,
                };

                data = context.ElementData;

                if (updater != null) {
                    part.ElementData = data.Serialize();
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.ElementWrapper", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override void Exporting(ElementWrapperPart part, ExportContentContext context) {
            var describeContext = CreateDescribeContext(part);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, part.ElementTypeName);
            var data = ElementDataHelper.Deserialize(part.ElementData);
            var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);

            _elementManager.Exporting(new[] { element }, new ExportLayoutContext());
            var exportableData = _serializer.Serialize(element);

            context.Element(part.PartDefinition.Name).SetValue(exportableData);
        }

        protected override void Importing(ElementWrapperPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);

            if (root == null)
                return;

            var exportedData = root.Value;
            var describeContext = CreateDescribeContext(part);
            var element = _serializer.Deserialize(exportedData, describeContext);

            _elementManager.Importing(new[]{element}, new ImportLayoutContext { Session = new ImportContentContextWrapper(context)});
            part.ElementData = element.Data.Serialize();
        }

        private static DescribeElementsContext CreateDescribeContext(IContent part) {
            return new DescribeElementsContext {
                Content = part
            };
        }

        private ElementEditorContext CreateEditorContext(IContent content, Element element, ElementDataDictionary elementData, IUpdateModel updater, dynamic shapeFactory) {
            var context = new ElementEditorContext {
                Content = content,
                Element = element,
                Updater = updater,
                ValueProvider = elementData.ToValueProvider(_cultureAccessor.CurrentCulture),
                ElementData = elementData,
                ShapeFactory = shapeFactory,
                Prefix = Prefix
            };
            return context;
        }
    }
}