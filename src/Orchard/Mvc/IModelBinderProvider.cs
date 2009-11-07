using System.Collections.Generic;

namespace Orchard.Mvc {
    public interface IModelBinderProvider : IDependency {
        IEnumerable<ModelBinderDescriptor> GetModelBinders();
    }
}
