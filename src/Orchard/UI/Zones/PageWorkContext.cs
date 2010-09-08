using System.Linq;
using ClaySharp.Implementation;
using Orchard.DisplayManagement;

namespace Orchard.UI.Zones {
    public class PageWorkContext : IWorkContextStateProvider {
        private readonly IShapeFactory _shapeFactory;

        public PageWorkContext(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public T Get<T>(string name) {
            if (name == "Page") {
                return (dynamic)_shapeFactory.Create("Layout", Arguments.Empty());
            }
            return default(T);
        }
    }
}
