using System.Collections.Generic;

namespace Orchard.Mvc.ModelBinders {
    public interface IModelBinderProvider : IDependency {
        IEnumerable<ModelBinderDescriptor> GetModelBinders();
    }
}