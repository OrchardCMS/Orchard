using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Orchard.DisplayManagement {
    public class DefaultShapeTableFactory : IShapeTableFactory {
        private readonly IEnumerable<IShapeProvider> _shapeProviders;

        public DefaultShapeTableFactory(IEnumerable<IShapeProvider> shapeProviders) {
            _shapeProviders = shapeProviders;
        }

        public ShapeTable CreateShapeTable() {
            var table = new ShapeTable { Entries = GetEntries().ToDictionary(e => e.ShapeType) };
            return table;
        }

        private IEnumerable<ShapeTable.Entry> GetEntries() {
            foreach (var shapeProvider in _shapeProviders) {
                foreach (var methodInfo in shapeProvider.GetType().GetMethods().Where(IsAcceptableMethod)) {
                    var info = methodInfo;
                    var provider = shapeProvider;
                    yield return new ShapeTable.Entry {
                        ShapeType = methodInfo.Name,
                        Target = ctx => PerformInvoke(ctx, info, provider)
                    };
                }
            }
        }

        private object PerformInvoke(DisplayContext displayContext, MethodInfo methodInfo, IShapeProvider shapeProvider) {
            // oversimplification for the sake of evolving
            dynamic shape = displayContext.Value;
            var arguments = methodInfo.GetParameters()
                .Select(parameter => BindParameter(displayContext, parameter));

            return methodInfo.Invoke(shapeProvider, arguments.ToArray());
        }

        private object BindParameter(DisplayContext displayContext, ParameterInfo parameter) {
            if (parameter.Name == "Shape")
                return displayContext.Value;

            if (parameter.Name == "Display")
                return displayContext.Display;

            return ((dynamic)(displayContext.Value))[parameter.Name];
        }

        static bool IsAcceptableMethod(MethodInfo methodInfo) {
            if (methodInfo.IsSpecialName)
                return false;
            if (methodInfo.DeclaringType == typeof(object))
                return false;
            return true;
        }
    }
}
