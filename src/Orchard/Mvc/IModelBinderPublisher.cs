using System.Collections.Generic;

namespace Orchard.Mvc {
    public interface IModelBinderPublisher : IDependency {
        void Publish(IEnumerable<ModelBinderDescriptor> binders);
    }
}