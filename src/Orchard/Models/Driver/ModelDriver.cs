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
        void IModelDriver.GetEditors(GetModelEditorsContext context) { GetEditors(context); }
        void IModelDriver.UpdateEditors(UpdateModelContext context) { UpdateEditors(context); }

        protected virtual void New(NewModelContext context) {
        }

        protected virtual void Load(LoadModelContext context) {
        }

        protected virtual void Create(CreateModelContext context) {
        }

        protected virtual void GetEditors(GetModelEditorsContext context) {
        }

        protected virtual void UpdateEditors(UpdateModelContext context) {
        }

        /// <summary>
        /// TODO: move this - make a modelbuilder class or something
        /// </summary>
        /// <typeparam name="TPart"></typeparam>
        /// <param name="context"></param>
        protected void WeldModelPart<TPart>(NewModelContext context) where TPart : class,IModel,new() {
            var newPart = new TPart();
            newPart.Weld(context.Instance);
            context.Instance = newPart;
        }

    }
}