using System;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Handlers {
    public class ElementDriversCoordinator : ElementEventHandlerBase {
        private readonly IElementManager _elementManager;
        public ElementDriversCoordinator(IElementManager elementManager) {
            _elementManager = elementManager;
        }

        public override void BuildEditor(ElementEditorContext context) {
            BuildEditorInternal(context, driver => driver.BuildEditor(context));
        }

        public override void UpdateEditor(ElementEditorContext context) {
            BuildEditorInternal(context, driver => driver.UpdateEditor(context));
        }

        public override void LayoutSaving(ElementSavingContext context) {
            InvokeDrivers(context.Element, driver => driver.LayoutSaving(context));
        }

        public override void Removing(ElementRemovingContext context) {
            InvokeDrivers(context.Element, driver => driver.Removing(context));
        }

        public override void Exporting(ExportElementContext context) {
            InvokeDrivers(context.Element, driver => driver.Exporting(context));
        }

        public override void Exported(ExportElementContext context) {
            InvokeDrivers(context.Element, driver => driver.Exported(context));
        }

        public override void Importing(ImportElementContext context) {
            InvokeDrivers(context.Element, driver => driver.Importing(context));
        }

        public override void Imported(ImportElementContext context) {
            InvokeDrivers(context.Element, driver => driver.Imported(context));
        }

        public override void ImportCompleted(ImportElementContext context) {
            InvokeDrivers(context.Element, driver => driver.ImportCompleted(context));
        }

        private void BuildEditorInternal(ElementEditorContext context, Func<IElementDriver, EditorResult> action) {
            var descriptor = context.Element.Descriptor;
            var drivers = _elementManager.GetDrivers(descriptor);

            foreach (var driver in drivers) {
                var editorResult = action(driver);

                if (editorResult == null)
                    continue;

                foreach (var editor in editorResult.Editors) {
                    editor.ElementDescriptor = descriptor;
                    editor.ElementData = context.Element.Data;
                    editor.Content = context.Content;

                    context.EditorResult.Add(editor);
                }
            }
        }

        private void InvokeDrivers(Element element, Action<IElementDriver> action) {
            var drivers = _elementManager.GetDrivers(element.Descriptor);
            drivers.Invoke(action, Logger);
        }
    }
}