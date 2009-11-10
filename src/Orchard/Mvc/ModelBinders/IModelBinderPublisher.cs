using System.Collections.Generic;

namespace Orchard.Mvc.ModelBinders {
    public interface IModelBinderPublisher : IDependency {
        void Publish(IEnumerable<ModelBinderDescriptor> binders);
    }
}