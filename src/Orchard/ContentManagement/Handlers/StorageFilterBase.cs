namespace Orchard.ContentManagement.Handlers {
    public abstract class StorageFilterBase<TPart> : IContentStorageFilter where TPart : class, IContent {

        protected virtual void Activated(ActivatedContentContext context, TPart instance) { }
        protected virtual void Initializing(InitializingContentContext context, TPart instance) { }
        protected virtual void Creating(CreateContentContext context, TPart instance) { }
        protected virtual void Created(CreateContentContext context, TPart instance) { }
        protected virtual void Loading(LoadContentContext context, TPart instance) { }
        protected virtual void Loaded(LoadContentContext context, TPart instance) { }
        protected virtual void Updating(UpdateContentContext context, TPart instance) { }
        protected virtual void Updated(UpdateContentContext context, TPart instance) { }
        protected virtual void Versioning(VersionContentContext context, TPart existing, TPart building) { }
        protected virtual void Versioned(VersionContentContext context, TPart existing, TPart building) { }
        protected virtual void Publishing(PublishContentContext context, TPart instance) { }
        protected virtual void Published(PublishContentContext context, TPart instance) { }
        protected virtual void Unpublishing(PublishContentContext context, TPart instance) { }
        protected virtual void Unpublished(PublishContentContext context, TPart instance) { }
        protected virtual void Removing(RemoveContentContext context, TPart instance) { }
        protected virtual void Removed(RemoveContentContext context, TPart instance) { }
        protected virtual void Indexing(IndexContentContext context, TPart instance) { }
        protected virtual void Indexed(IndexContentContext context, TPart instance) { }


        void IContentStorageFilter.Activated(ActivatedContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Activated(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Initializing(InitializingContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Initializing(context, context.ContentItem.As<TPart>());
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

        void IContentStorageFilter.Updating(UpdateContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Updating(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Updated(UpdateContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Updated(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Versioning(VersionContentContext context) {
            if (context.ExistingContentItem.Is<TPart>() || context.BuildingContentItem.Is<TPart>())
                Versioning(context, context.ExistingContentItem.As<TPart>(), context.BuildingContentItem.As<TPart>());
        }

        void IContentStorageFilter.Versioned(VersionContentContext context) {
            if (context.ExistingContentItem.Is<TPart>() || context.BuildingContentItem.Is<TPart>())
                Versioned(context, context.ExistingContentItem.As<TPart>(), context.BuildingContentItem.As<TPart>());
        }

        void IContentStorageFilter.Publishing(PublishContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Publishing(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Published(PublishContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Published(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Unpublishing(PublishContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Unpublishing(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Unpublished(PublishContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Unpublished(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Removing(RemoveContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Removing(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Removed(RemoveContentContext context) {
            if (context.ContentItem.Is<TPart>())
                Removed(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Indexing(IndexContentContext context) {
            if ( context.ContentItem.Is<TPart>() )
                Indexing(context, context.ContentItem.As<TPart>());
        }

        void IContentStorageFilter.Indexed(IndexContentContext context) {
            if ( context.ContentItem.Is<TPart>() )
                Indexed(context, context.ContentItem.As<TPart>());
        }

    }
}
