using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Models {
    public class DefaultModelManager : IModelManager {
        private readonly IEnumerable<IModelDriver> _drivers;
        private readonly IRepository<ModelRecord> _modelRepository;

        public DefaultModelManager(
            IEnumerable<IModelDriver> drivers,
            IRepository<ModelRecord> modelRepository) {
            _drivers = drivers;
            _modelRepository = modelRepository;
        }

        public virtual IModel New(string modelType) {

            // create a new kernel for the model instance
            var context = new NewModelContext {
                ModelType = modelType,
                Instance = new ModelRoot(modelType)
            };

            // invoke drivers to weld aspects onto kernel
            foreach (var driver in _drivers) {
                driver.New(context);
            }

            // composite result is returned
            return context.Instance;
        }

        public virtual IModel Get(int id) {
            // obtain root record to determine the model type
            var modelRecord = _modelRepository.Get(id);

            // create a context with a new instance to load
            var context = new LoadModelContext {
                Id = modelRecord.Id,
                ModelType = modelRecord.ModelType.Name,
                Instance = New(modelRecord.ModelType.Name)
            };

            // set the id
            context.Instance.As<ModelRoot>().Id = context.Id;

            // invoke drivers to weld aspects onto kernel
            foreach (var driver in _drivers) {
                driver.Load(context);
            }

            return context.Instance;
        }
    }
}
