using System.Web.Mvc;

namespace Orchard.Layouts.Services {
    public interface ICurrentControllerAccessor : IDependency {
        Controller CurrentController { get; }
    }
}