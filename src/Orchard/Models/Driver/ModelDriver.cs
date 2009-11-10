using System;
using Orchard.Logging;

namespace Orchard.Models.Driver {
    public abstract class ModelDriver : IModelDriver {
        protected ModelDriver() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger{ get; set;}

        void IModelDriver.New(NewModelContext context) {New(context);}
        void IModelDriver.Create(CreateModelContext context) { Create(context); }
        void IModelDriver.Load(LoadModelContext context) { Load(context); }

        protected virtual void New(NewModelContext context) {
        }

        protected virtual void Load(LoadModelContext context) {
        }

        protected virtual void Create(CreateModelContext context) {
        }

        protected void WeldModelPart<TPart>(NewModelContext context) where TPart : class,IModel,new() {
            var newPart = new TPart();
            newPart.Weld(context.Instance);
            context.Instance = newPart;
        }
    }
}