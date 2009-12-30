namespace Orchard.ContentManagement.Handlers {
    public abstract class StorageFilterBase<TPart> : IContentStorageFilter where TPart : class, IContent {

        protected virtual void Activated(ActivatedContentContext context, TPart instance) { }
        protected virtual void Creating(CreateContentContext context, TPart instance) { }
        protected virtual void Created(CreateContentContext context, TPart instance) { }
        protected virtual void Loading(LoadContentContext context, TPart instance) { }
        protected virtual void Loaded(LoadContentContext context, TPart instance) { }
        protected virtual void Versioning(VersionContentContext context, TPart existing, TPart building) { }
        protected virtual void Versioned(VersionContentContext context, TPart existing, TPart building) { }
        protected virtual void Removing(RemoveContentContext context, TPart instance) { }
        protected virtual void Removed(RemoveContentContext context, TPart instance) { }


        void IContentStorageFilter.Activated(ActivatedContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Activated(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Creating(CreateContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Creating(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Created(CreateContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Created(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Loading(LoadContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Loading(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Loaded(LoadContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Loaded(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Versioning(VersionContentContext context) {
            if (context.ExistingContentItem.Is<TPart>() || context.BuildingContentItem.Is<TPart>())
                Versioning(context, context.ExistingContentItem.As<TPart>(), context.BuildingContentItem.As<TPart>());
        }

        void IContentStorageFilter.Versioned(VersionContentContext context) {
            if (context.ExistingContentItem.Is<TPart>() || context.BuildingContentItem.Is<TPart>())
                Versioned(context, context.ExistingContentItem.As<TPart>(), context.BuildingContentItem.As<TPart>());
        }

        void IContentStorageFilter.Removing(RemoveContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Removing(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Removed(RemoveContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Removed(context, context.ContentItem.As<TPart>());
        }
    }
}
