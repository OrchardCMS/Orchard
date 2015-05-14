using System;
using Orchard.Layouts.Framework.Drivers;
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
    }
}