using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Mvc {
    public class ModelBinderPublisher : IModelBinderPublisher {
        private readonly ModelBinderDictionary _binders;

        public ModelBinderPublisher(ModelBinderDictionary binders) {
            _binders = binders;
        }

        public void Publish(IEnumerable<ModelBinderDescriptor> binders) {
            foreach (var descriptor in binders) {
                _binders.Add(descriptor.Type, descriptor.ModelBinder);
            }
        }
    }
}
