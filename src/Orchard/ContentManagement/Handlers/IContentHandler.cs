namespace Orchard.ContentManagement.Handlers {
    /*
     * * These are the various methods that run during content lifecycle. Below are the calls in order
     * * when ContentManager methods are used to alter content.
     * * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * * ContentManager.New                                                                *
     * * Activating -> Activated -> Initializing                                           *                 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * * Contentmanager.Create                                                             *
     * * Activating -> Activated -> Initializing -> Creating -> Created                    *
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * *
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * * ContentManager.Get                                                                *
     * * Activating -> Activated -> Initializing -> Loading -> Loaded                      * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * *
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * * ContentManager.Get (if latest is published)                                       *
     * * On the existing item:                                                             *
     * * Activating -> Activated -> Initializing -> Loading -> Loaded                      *
     * * On the allocated item:                                                            *
     * * Activating -> Activated -> Initializing -> Versioning -> Versioned                *
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     */
    public interface IContentHandler : IDependency {
        // When parts are welded via builder.
        void Activating(ActivatingContentContext context);
        // After newly allocated parts have been built. 
        void Activated(ActivatedContentContext context);

        // Phase where defaults are set.
        void Initializing(InitializingContentContext context);

        // Called during Create of new ContentItem id.
        void Creating(CreateContentContext context);
        void Created(CreateContentContext context);

        // Called during Get, Query, etc to redydrate item (item produced by New).
        void Loading(LoadContentContext context);
        void Loaded(LoadContentContext context);

        // Called to populate/refine new records produced when versioning existing item (existing item previously Loaded).
        void Versioning(VersionContentContext context);
        void Versioned(VersionContentContext context);

        // Called when Publishing/Unpublishing via the ContentManager.
        void Publishing(PublishContentContext context);
        void Published(PublishContentContext context);
        void Unpublishing(PublishContentContext context);
        void Unpublished(PublishContentContext context);

        // Called from ContentManager.Remove prior to removal.
        void Removing(RemoveContentContext context);
        // Called from ContentManager.Remove right after removal.
        void Removed(RemoveContentContext context);

        // Called when altering the index via ContentManager.Index to receive index information.
        void Indexing(IndexContentContext context);
        void Indexed(IndexContentContext context);

        // Called when importing/exporting content item data via ContentManager.Import or ContentManager.Export.
        void Importing(ImportContentContext context);
        void Imported(ImportContentContext context);
        void Exporting(ExportContentContext context);
        void Exported(ExportContentContext context);

        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplay(BuildDisplayContext context);
        void BuildEditor(BuildEditorContext context);
        void UpdateEditor(UpdateEditorContext context);
    }
}
