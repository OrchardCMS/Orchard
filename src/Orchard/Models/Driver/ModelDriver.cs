using Orchard.Data;
using Orchard.Logging;

namespace Orchard.Models.Driver {
    public class ModelDriver : IModelDriver {
        protected ModelDriver() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger{ get; set;}

        void IModelDriver.New(NewModelContext context) {New(context);}
        void IModelDriver.Load(LoadModelContext context) {Load(context);}

        protected virtual void New(NewModelContext context) {
        }

        protected virtual void Load(LoadModelContext context) {
        }

        protected void WeldModelPart<TPart>(NewModelContext context) where TPart : class,IModel,new() {
            var newPart = new TPart();
            newPart.Weld(context.Instance);
            context.Instance = newPart;
        }
    }

    public class ModelDriver<TRecord> : ModelDriver {
        private readonly IRepository<TRecord> _repository;

        public ModelDriver(IRepository<TRecord> repository) {
            _repository = repository;
        }

        protected override void Load(LoadModelContext context) {
            var instance = context.Instance.As<ModelPart<TRecord>>();
            if (instance != null)
                instance.Record = _repository.Get(context.Id);

            base.Load(context);
        }
    }
}