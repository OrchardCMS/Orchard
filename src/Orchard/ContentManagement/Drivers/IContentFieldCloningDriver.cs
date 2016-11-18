using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentFieldCloningDriver : IDependency, IContentFieldDriver {
        void Cloning(CloneContentContext context);
        void Cloned(CloneContentContext context);
    }
}
