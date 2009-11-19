namespace Orchard.Models.Driver {
    public abstract class StorageFilterBase<TPart> : IModelStorageFilter where TPart : class, IContentItemPart {

        protected virtual void Activated(ActivatedModelContext context, TPart instance) { }
        protected virtual void Creating(CreateModelContext context, TPart instance) { }
        protected virtual void Created(CreateModelContext context, TPart instance) { }
        protected virtual void Loading(LoadModelContext context, TPart instance) { }
        protected virtual void Loaded(LoadModelContext context, TPart instance) { }


        void IModelStorageFilter.Activated(ActivatedModelContext context) {
            if (context.ContentItem.Is<TPart>())
                Activated(context, context.ContentItem.As<TPart>());
        }

        void IModelStorageFilter.Creating(CreateModelContext context) {
            if (context.ContentItem.Is<TPart>())
                Creating(context, context.ContentItem.As<TPart>());
        }

        void IModelStorageFilter.Created(CreateModelContext context) {
            if (context.ContentItem.Is<TPart>())
                Created(context, context.ContentItem.As<TPart>());
        }

        void IModelStorageFilter.Loading(LoadModelContext context) {
            if (context.ContentItem.Is<TPart>())
                Loading(context, context.ContentItem.As<TPart>());
        }

        void IModelStorageFilter.Loaded(LoadModelContext context) {
            if (context.ContentItem.Is<TPart>())
                Loaded(context, context.ContentItem.As<TPart>());
        }
    }
}
