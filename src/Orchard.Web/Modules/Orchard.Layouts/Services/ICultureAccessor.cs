using System.Globalization;

namespace Orchard.Layouts.Services {
    public interface ICultureAccessor : IDependency {
        CultureInfo CurrentCulture { get; }
    }
}