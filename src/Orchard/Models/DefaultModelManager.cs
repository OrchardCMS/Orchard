using System.Collections.Generic;
using Orchard.Models.Driver;

namespace Orchard.Models {
    public class DefaultModelManager : IModelManager {
        private readonly IEnumerable<IModelDriver> _drivers;

        public DefaultModelManager(IEnumerable<IModelDriver> drivers) {
            _drivers = drivers;
        }

        public virtual IModel New(string modelType) {
            
            // create a new kernel for the model instance
            var context = new NewModelContext {
                ModelType = modelType,
                Instance = new ModelRoot(modelType)
            };

            // invoke drivers to weld aspects onto kernel
            foreach(var driver in _drivers) {
                driver.New(context);
            }

            // composite result is returned
            return context.Instance;
        }
    }
}
