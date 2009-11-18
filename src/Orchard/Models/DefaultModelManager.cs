using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using Orchard.Data;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.UI.Models;

namespace Orchard.Models {
    public class DefaultModelManager : IModelManager {
        private readonly IContext _context;
        private readonly IRepository<ModelRecord> _modelRepository;
        private readonly IRepository<ModelTypeRecord> _modelTypeRepository;

        public DefaultModelManager(
            IContext context,
            IRepository<ModelRecord> modelRepository,
            IRepository<ModelTypeRecord> modelTypeRepository) {
            _context = context;
            _modelRepository = modelRepository;
            _modelTypeRepository = modelTypeRepository;
        }

        private IEnumerable<IModelDriver> _drivers;
        public IEnumerable<IModelDriver> Drivers {
            get {
                if (_drivers == null)
                    _drivers = _context.Resolve<IEnumerable<IModelDriver>>();
                return _drivers;
            }            
        }

        public virtual IModel New(string modelType) {

            // create a new kernel for the model instance
            var context = new ActivatingModelContext {
                ModelType = modelType,
                Builder = new ModelBuilder(modelType)
            };

            // invoke drivers to weld aspects onto kernel
            foreach (var driver in Drivers) {
                driver.Activating(context);
            }
            var context2 = new ActivatedModelContext {
                ModelType = modelType,
                Instance = context.Builder.Build()
            };
            foreach (var driver in Drivers) {
                driver.Activated(context2);
            }

            // composite result is returned
            return context2.Instance;
        }

        public virtual IModel Get(int id) {
            // obtain root record to determine the model type
            var modelRecord = _modelRepository.Get(id);

            // create a context with a new instance to load
            var context = new LoadModelContext {
                Id = modelRecord.Id,
                ModelType = modelRecord.ModelType.Name,
                Record = modelRecord,
                Instance = New(modelRecord.ModelType.Name)
            };

            // set the id
            context.Instance.As<ModelRoot>().Id = context.Id;

            // invoke drivers to acquire state, or at least establish lazy loading callbacks
            foreach (var driver in Drivers) {
                driver.Loading(context);
            }
            foreach (var driver in Drivers) {
                driver.Loaded(context);
            }

            return context.Instance;
        }

        public void Create(IModel model) {
            // produce root record to determine the model id
            var modelRecord = new ModelRecord { ModelType = AcquireModelTypeRecord(model.ModelType) };
            _modelRepository.Create(modelRecord);

            // build a context with the initialized instance to create
            var context = new CreateModelContext {
                Id = modelRecord.Id,
                ModelType = modelRecord.ModelType.Name,
                Record = modelRecord,
                Instance = model.As<ModelRoot>().Welded
            };

            // set the id
            context.Instance.As<ModelRoot>().Id = context.Id;


            // invoke drivers to add information to persistent stores
            foreach (var driver in Drivers) {
                driver.Creating(context);
            }
            foreach (var driver in Drivers) {
                driver.Created(context);
            }
        }

        public IEnumerable<ModelTemplate> GetEditors(IModel model) {
            var context = new GetModelEditorsContext(model);
            foreach (var driver in Drivers) {
                driver.GetEditors(context);
            }
            return context.Editors;
        }

        public IEnumerable<ModelTemplate> UpdateEditors(IModel model, IModelUpdater updater) {
            var context = new UpdateModelContext(model, updater);
            foreach (var driver in Drivers) {
                driver.UpdateEditors(context);
            }
            return context.Editors;
        }

        private ModelTypeRecord AcquireModelTypeRecord(string modelType) {
            var modelTypeRecord = _modelTypeRepository.Get(x => x.Name == modelType);
            if (modelTypeRecord == null) {
                //TEMP: this is not safe... Model types could be created concurrently?
                modelTypeRecord = new ModelTypeRecord { Name = modelType };
                _modelTypeRepository.Create(modelTypeRecord);
            }
            return modelTypeRecord;
        }
    }
}
