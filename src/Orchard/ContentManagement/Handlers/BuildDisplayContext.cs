using System;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildDisplayContext : BuildShapeContext {
        public BuildDisplayContext(IShape model, IContent content, string displayType, string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory) {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }
        public dynamic NewShapeWithTypeName(string shapeTypeName) {
            return _shapeHelperCalls.Invoke(New, shapeTypeName);
        }

        private static readonly CallSiteCollection _shapeHelperCalls = new CallSiteCollection(shapeTypeName => Binder.InvokeMember(
            CSharpBinderFlags.None,
            shapeTypeName,
            Enumerable.Empty<Type>(),
            null,
            new[] {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)}));

    }
}
