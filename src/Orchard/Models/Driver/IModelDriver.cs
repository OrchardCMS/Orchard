using Orchard.Logging;

namespace Orchard.Models.Driver {
    public interface IModelDriver : IDependency {
        void New(NewModelContext context);
    }

    public abstract class ModelDriverBase : IModelDriver {
        protected ModelDriverBase() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger{ get; set;}

        void IModelDriver.New(NewModelContext context) {New(context);}

        protected virtual void New(NewModelContext context) {
        }

        protected void WeldModelPart<TPart>(NewModelContext context) where TPart : class,IModel,new() {
            var newPart = new TPart();
            newPart.Weld(context.Instance);
            context.Instance = newPart;
        }

    }
}