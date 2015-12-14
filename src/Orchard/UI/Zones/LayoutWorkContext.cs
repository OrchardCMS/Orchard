using System;
using Orchard.DisplayManagement;

namespace Orchard.UI.Zones {
    public class LayoutWorkContext : IWorkContextStateProvider {
        private readonly IShapeFactory _shapeFactory;

        public LayoutWorkContext(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public Func<WorkContext, T> Get<T>(string name) {
            if (name == "Layout") {
                var layout = _shapeFactory.Create("Layout", Arguments.Empty());
                return ctx => (T)layout;
            }
            return null;
        }
    }
}
