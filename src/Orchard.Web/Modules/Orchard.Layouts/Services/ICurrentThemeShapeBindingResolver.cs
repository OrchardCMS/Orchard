using System;

namespace Orchard.Layouts.Services {
    public interface ICurrentThemeShapeBindingResolver : IDependency {
        IDisposable Enable();
    }
}