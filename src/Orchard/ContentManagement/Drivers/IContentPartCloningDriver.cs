using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentPartCloningDriver : IDependency, IContentPartDriver {
        void Cloning(CloneContentContext context);
        void Cloned(CloneContentContext context);
    }
}
