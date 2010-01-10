namespace Orchard.ContentManagement.Handlers {
    public interface IContentStorageFilter : IContentFilter {
        void Activated(ActivatedContentContext context);
        void Creating(CreateContentContext context);
        void Created(CreateContentContext context);
        void Loading(LoadContentContext context);
        void Loaded(LoadContentContext context);
        void Versioning(VersionContentContext context);
        void Versioned(VersionContentContext context);
        void Publishing(PublishContentContext context);
        void Published(PublishContentContext context);
        void Removing(RemoveContentContext context);
        void Removed(RemoveContentContext context);
    }
}
