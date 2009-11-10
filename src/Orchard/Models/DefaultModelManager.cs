using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Models {
    public class DefaultModelManager : IModelManager {
        private readonly IEnumerable<IModelDriver> _drivers;
        private readonly IRepository<ModelRecord> _modelRepository;
        private readonly IRepository<ModelTypeRecord> _modelTypeRepository;

        public DefaultModelManager(
            IEnumerable<IModelDriver> drivers,
            IRepository<ModelRecord> modelRepository,
            IRepository<ModelTypeRecord> modelTypeRepository) {
            _drivers = drivers;
            _modelRepository = modelRepository;
            _modelTypeRepository = modelTypeRepository;
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

            // invoke drivers to acquire state, or at least establish lazy loading callbacks
            foreach (var driver in _drivers) {
                driver.Load(context);
            }

            return context.Instance;
        }

        public void Create(IModel model) {
            // produce root record to determine the model id
            var modelRecord = new ModelRecord {ModelType = AcquireModelTypeRecord(model.ModelType)};
            _modelRepository.Create(modelRecord);

            // build a context with the initialized instance to create
            var context = new CreateModelContext {
                Id = modelRecord.Id,
                ModelType = modelRecord.ModelType.Name,
                Instance = model.As<ModelRoot>().Welded
            };

            // set the id
            context.Instance.As<ModelRoot>().Id = context.Id;


            // invoke drivers to add information to persistent stores
            foreach (var driver in _drivers) {
                driver.Create(context);
            }
        }

        private ModelTypeRecord AcquireModelTypeRecord(string modelType) {
            var modelTypeRecord = _modelTypeRepository.Get(x => x.Name == modelType);
            if (modelTypeRecord == null) {
                //TEMP: this is not safe... Model types could be created concurrently?
                modelTypeRecord = new ModelTypeRecord {Name = modelType};
                _modelTypeRepository.Create(modelTypeRecord);
            }
            return modelTypeRecord;
        }
    }
}
