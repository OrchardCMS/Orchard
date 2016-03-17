namespace Orchard.ContentManagement.Handlers {
    public interface IContentHandler : IDependency {
        void Activating(ActivatingContentContext context);
        void Activated(ActivatedContentContext context);
        void Initializing(InitializingContentContext context);
        void Initialized(InitializingContentContext context);
        void Creating(CreateContentContext context);
        void Created(CreateContentContext context);
        void Loading(LoadContentContext context);
        void Loaded(LoadContentContext context);
        void Updating(UpdateContentContext context);
        void Updated(UpdateContentContext context);
        void Versioning(VersionContentContext context);
        void Versioned(VersionContentContext context);
        void Publishing(PublishContentContext context);
        void Published(PublishContentContext context);
        void Unpublishing(PublishContentContext context);
        void Unpublished(PublishContentContext context);
        void Removing(RemoveContentContext context);
        void Removed(RemoveContentContext context);
        void Indexing(IndexContentContext context);
        void Indexed(IndexContentContext context);
        void Importing(ImportContentContext context);
        void Imported(ImportContentContext context);
        void Cloning(CloneContentContext context);
        void Cloned(CloneContentContext context);
        void ImportCompleted(ImportContentContext importContentContext);
        void Exporting(ExportContentContext context);
        void Exported(ExportContentContext context);

        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplay(BuildDisplayContext context);
        void BuildEditor(BuildEditorContext context);
        void UpdateEditor(UpdateEditorContext context);
        void Restoring(RestoreContentContext context);
        void Restored(RestoreContentContext context);
        void Destroying(DestroyContentContext context);
        void Destroyed(DestroyContentContext context);
    }
}
