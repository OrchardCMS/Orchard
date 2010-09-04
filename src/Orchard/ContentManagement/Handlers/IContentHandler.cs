using System.Collections.Generic;

namespace Orchard.ContentManagement.Handlers {
    public interface IContentHandler : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        void Activating(ActivatingContentContext context);
        void Activated(ActivatedContentContext context);
        void Initializing(InitializingContentContext context);
        void Creating(CreateContentContext context);
        void Created(CreateContentContext context);
        void Saving(SaveContentContext context);
        void Saved(SaveContentContext context);
        void Loading(LoadContentContext context);
        void Loaded(LoadContentContext context);
        void Versioning(VersionContentContext context);
        void Versioned(VersionContentContext context);
        void Publishing(PublishContentContext context);
        void Published(PublishContentContext context);
        void Removing(RemoveContentContext context);
        void Removed(RemoveContentContext context);
        void Indexing(IndexContentContext context);
        void Indexed(IndexContentContext context);

        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplayModel(BuildDisplayModelContext context);
        void BuildEditorModel(BuildEditorModelContext context);
        void UpdateEditorModel(UpdateEditorModelContext context);
    }
}
