using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;

namespace Orchard.ContentManagement.Handlers {
    public abstract class ContentHandler : IContentHandler {
        protected ContentHandler() {
            Filters = new List<IContentFilter>();
            Logger = NullLogger.Instance;
        }

        public List<IContentFilter> Filters { get; set; }
        public ILogger Logger { get; set; }

        protected void OnActivated<TPart>(Action<ActivatedContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnActivated = handler });
        }

        protected void OnCreating<TPart>(Action<CreateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnCreating = handler });
        }

        protected void OnCreated<TPart>(Action<CreateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnCreated = handler });
        }

        protected void OnLoading<TPart>(Action<LoadContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnLoading = handler });
        }

        protected void OnLoaded<TPart>(Action<LoadContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnLoaded = handler });
        }

        protected void OnVersioning<TPart>(Action<VersionContentContext, TPart, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnVersioning = handler });
        }

        protected void OnVersioned<TPart>(Action<VersionContentContext, TPart, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnVersioned = handler });
        }

        protected void OnPublishing<TPart>(Action<PublishContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnPublishing = handler });
        }

        protected void OnPublished<TPart>(Action<PublishContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnPublished = handler });
        }

        protected void OnRemoving<TPart>(Action<RemoveContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnRemoving = handler });
        }

        protected void OnRemoved<TPart>(Action<RemoveContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnRemoved = handler });
        }

        protected void OnIndexing<TPart>(Action<IndexContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnIndexing = handler });
        }

        protected void OnIndexed<TPart>(Action<IndexContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnIndexed = handler });
        }

        protected void OnGetContentItemMetadata<TPart>(Action<GetContentItemMetadataContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnGetItemMetadata = handler });
        }
        protected void OnGetDisplayViewModel<TPart>(Action<BuildDisplayModelContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnGetDisplayViewModel = handler });
        }

        protected void OnGetEditorViewModel<TPart>(Action<BuildEditorModelContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnGetEditorViewModel = handler });
        }

        protected void OnUpdateEditorViewModel<TPart>(Action<UpdateEditorModelContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnUpdateEditorViewModel = handler });
        }

        class InlineStorageFilter<TPart> : StorageFilterBase<TPart> where TPart : class, IContent {
            public Action<ActivatedContentContext, TPart> OnActivated { get; set; }
            public Action<CreateContentContext, TPart> OnCreating { get; set; }
            public Action<CreateContentContext, TPart> OnCreated { get; set; }
            public Action<LoadContentContext, TPart> OnLoading { get; set; }
            public Action<LoadContentContext, TPart> OnLoaded { get; set; }
            public Action<VersionContentContext, TPart, TPart> OnVersioning { get; set; }
            public Action<VersionContentContext, TPart, TPart> OnVersioned { get; set; }
            public Action<PublishContentContext, TPart> OnPublishing { get; set; }
            public Action<PublishContentContext, TPart> OnPublished { get; set; }
            public Action<RemoveContentContext, TPart> OnRemoving { get; set; }
            public Action<RemoveContentContext, TPart> OnRemoved { get; set; }
            public Action<IndexContentContext, TPart> OnIndexing { get; set; }
            public Action<IndexContentContext, TPart> OnIndexed { get; set; }
            protected override void Activated(ActivatedContentContext context, TPart instance) {
                if (OnActivated != null) OnActivated(context, instance);
            }
            protected override void Creating(CreateContentContext context, TPart instance) {
                if (OnCreating != null) OnCreating(context, instance);
            }
            protected override void Created(CreateContentContext context, TPart instance) {
                if (OnCreated != null) OnCreated(context, instance);
            }
            protected override void Loading(LoadContentContext context, TPart instance) {
                if (OnLoading != null) OnLoading(context, instance);
            }
            protected override void Loaded(LoadContentContext context, TPart instance) {
                if (OnLoaded != null) OnLoaded(context, instance);
            }
            protected override void Versioning(VersionContentContext context, TPart existing, TPart building) {
                if (OnVersioning != null) OnVersioning(context, existing, building);
            }
            protected override void Versioned(VersionContentContext context, TPart existing, TPart building) {
                if (OnVersioned != null) OnVersioned(context, existing, building);
            }
            protected override void Publishing(PublishContentContext context, TPart instance) {
                if (OnPublishing != null) OnPublishing(context, instance);
            }
            protected override void Published(PublishContentContext context, TPart instance) {
                if (OnPublished != null) OnPublished(context, instance);
            }
            protected override void Removing(RemoveContentContext context, TPart instance) {
                if (OnRemoving != null) OnRemoving(context, instance);
            }
            protected override void Removed(RemoveContentContext context, TPart instance) {
                if (OnRemoved != null) OnRemoved(context, instance);
            }
            protected override void Indexing(IndexContentContext context, TPart instance) {
                if ( OnIndexing != null )
                    OnIndexing(context, instance);
            }
            protected override void Indexed(IndexContentContext context, TPart instance) {
                if ( OnIndexed != null )
                    OnIndexed(context, instance);
            }

        }

        class InlineTemplateFilter<TPart> : TemplateFilterBase<TPart> where TPart : class, IContent {
            public Action<GetContentItemMetadataContext, TPart> OnGetItemMetadata { get; set; }
            public Action<BuildDisplayModelContext, TPart> OnGetDisplayViewModel { get; set; }
            public Action<BuildEditorModelContext, TPart> OnGetEditorViewModel { get; set; }
            public Action<UpdateEditorModelContext, TPart> OnUpdateEditorViewModel { get; set; }
            protected override void GetContentItemMetadata(GetContentItemMetadataContext context, TPart instance) {
                if (OnGetItemMetadata != null) OnGetItemMetadata(context, instance);
            }
            protected override void BuildDisplayModel(BuildDisplayModelContext context, TPart instance) {
                if (OnGetDisplayViewModel != null) OnGetDisplayViewModel(context, instance);
            }
            protected override void BuildEditorModel(BuildEditorModelContext context, TPart instance) {
                if (OnGetEditorViewModel != null) OnGetEditorViewModel(context, instance);
            }
            protected override void UpdateEditorModel(UpdateEditorModelContext context, TPart instance) {
                if (OnUpdateEditorViewModel != null) OnUpdateEditorViewModel(context, instance);
            }
        }

        public virtual IEnumerable<ContentType> GetContentTypes() {
            return Enumerable.Empty<ContentType>();
        }

        void IContentHandler.Activating(ActivatingContentContext context) {
            foreach (var filter in Filters.OfType<IContentActivatingFilter>())
                filter.Activating(context);
            Activating(context);
        }

        void IContentHandler.Activated(ActivatedContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Activated(context);
            Activated(context);
        }

        void IContentHandler.Creating(CreateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Creating(context);
            Creating(context);
        }

        void IContentHandler.Created(CreateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Created(context);
            Created(context);
        }

        void IContentHandler.Loading(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loading(context);
            Loading(context);
        }

        void IContentHandler.Loaded(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loaded(context);
            Loaded(context);
        }

        void IContentHandler.Versioning(VersionContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Versioning(context);
            Versioning(context);
        }

        void IContentHandler.Versioned(VersionContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Versioned(context);
            Versioned(context);
        }

        void IContentHandler.Publishing(PublishContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Publishing(context);
            Publishing(context);
        }

        void IContentHandler.Published(PublishContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Published(context);
            Published(context);
        }

        void IContentHandler.Removing(RemoveContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Removing(context);
            Removing(context);
        }

        void IContentHandler.Removed(RemoveContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Removed(context);
            Removed(context);
        }

        void IContentHandler.Indexing(IndexContentContext context) {
            foreach ( var filter in Filters.OfType<IContentStorageFilter>() )
                filter.Indexing(context);
            Indexing(context);
        }

        void IContentHandler.Indexed(IndexContentContext context) {
            foreach ( var filter in Filters.OfType<IContentStorageFilter>() )
                filter.Indexed(context);
            Indexed(context);
        }

        void IContentHandler.GetContentItemMetadata(GetContentItemMetadataContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.GetContentItemMetadata(context);
            GetItemMetadata(context);
        }
        void IContentHandler.BuildDisplayModel(BuildDisplayModelContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.BuildDisplayModel(context);
            BuildDisplayModel(context);
        }
        void IContentHandler.BuildEditorModel(BuildEditorModelContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.BuildEditorModel(context);
            BuildEditorModel(context);
        }
        void IContentHandler.UpdateEditorModel(UpdateEditorModelContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.UpdateEditorModel(context);
            UpdateEditorModel(context);
        }

        protected virtual void Activating(ActivatingContentContext context) { }
        protected virtual void Activated(ActivatedContentContext context) { }

        protected virtual void Creating(CreateContentContext context) { }
        protected virtual void Created(CreateContentContext context) { }

        protected virtual void Loading(LoadContentContext context) { }
        protected virtual void Loaded(LoadContentContext context) { }

        protected virtual void Versioning(VersionContentContext context) { }
        protected virtual void Versioned(VersionContentContext context) { }

        protected virtual void Publishing(PublishContentContext context) { }
        protected virtual void Published(PublishContentContext context) { }

        protected virtual void Removing(RemoveContentContext context) { }
        protected virtual void Removed(RemoveContentContext context) { }

        protected virtual void Indexing(IndexContentContext context) { }
        protected virtual void Indexed(IndexContentContext context) { }

        protected virtual void GetItemMetadata(GetContentItemMetadataContext context) { }
        protected virtual void BuildDisplayModel(BuildDisplayModelContext context) { }
        protected virtual void BuildEditorModel(BuildEditorModelContext context) { }
        protected virtual void UpdateEditorModel(UpdateEditorModelContext context) { }
    }
}