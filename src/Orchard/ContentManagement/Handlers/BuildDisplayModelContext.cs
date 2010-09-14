using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildDisplayModelContext : BuildModelContext {
        public BuildDisplayModelContext(IContent content, string displayType, IShape model, IShapeHelperFactory shapeHelperFactory)
            : base(content, model, shapeHelperFactory) {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }
    }
}
