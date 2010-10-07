using ClaySharp.Implementation;
using Orchard.DisplayManagement;

namespace Orchard.UI.Zones {
    public class LayoutWorkContext : IWorkContextStateProvider {
        private readonly IShapeFactory _shapeFactory;

        public LayoutWorkContext(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public T Get<T>(string name) {
            if (name == "Layout") {
                return (dynamic)_shapeFactory.Create("Layout", Arguments.Empty());
            }
            return default(T);
        }
    }
}
