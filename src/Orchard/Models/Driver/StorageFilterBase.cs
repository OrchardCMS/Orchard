namespace Orchard.Models.Driver {
    public abstract class StorageFilterBase<TPart> : IModelStorageFilter where TPart : class, IModel {

        protected virtual void Activated(ActivatedModelContext context, TPart instance) { }
        protected virtual void Creating(CreateModelContext context, TPart instance) { }
        protected virtual void Created(CreateModelContext context, TPart instance) { }
        protected virtual void Loading(LoadModelContext context, TPart instance) { }
        protected virtual void Loaded(LoadModelContext context, TPart instance) { }


        void IModelStorageFilter.Activated(ActivatedModelContext context) {
            if (context.Instance.Is<TPart>())
                Activated(context, context.Instance.As<TPart>());
        }

        void IModelStorageFilter.Creating(CreateModelContext context) {
            if (context.Instance.Is<TPart>())
                Creating(context, context.Instance.As<TPart>());
        }

        void IModelStorageFilter.Created(CreateModelContext context) {
            if (context.Instance.Is<TPart>())
                Created(context, context.Instance.As<TPart>());
        }

        void IModelStorageFilter.Loading(LoadModelContext context) {
            if (context.Instance.Is<TPart>())
                Loading(context, context.Instance.As<TPart>());
        }

        void IModelStorageFilter.Loaded(LoadModelContext context) {
            if (context.Instance.Is<TPart>())
                Loaded(context, context.Instance.As<TPart>());
        }
    }
}
