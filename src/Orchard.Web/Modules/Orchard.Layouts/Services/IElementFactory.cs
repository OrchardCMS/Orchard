using System;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public interface IElementFactory : IDependency {
        IElement Activate(Type elementType);
        T Activate<T>() where T:IElement;
        IElement Activate(ElementDescriptor descriptor, ActivateElementArgs args = null);
    }
}