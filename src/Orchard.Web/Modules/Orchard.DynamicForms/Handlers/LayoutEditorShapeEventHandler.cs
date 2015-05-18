using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.UI.Resources;

namespace Orchard.DynamicForms.Handlers {
    public class LayoutEditorShapeEventHandler : IShapeTableProvider {
        private readonly Work<IResourceManager> _resourceManager;
        public LayoutEditorShapeEventHandler(Work<IResourceManager> resourceManager) {
            _resourceManager = resourceManager;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("EditorTemplate").OnDisplaying(context => {
                if (context.Shape.TemplateName != "Parts.Layout")
                    return;

                _resourceManager.Value.Require("stylesheet", "DynamicForms.FormElements");
                _resourceManager.Value.Require("script", "DynamicForms.FormElements");
            });
        }
    }
}