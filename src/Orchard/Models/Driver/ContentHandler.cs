using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;

namespace Orchard.Models.Driver {
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

        protected void OnGetItemMetadata<TPart>(Action<GetItemMetadataContext, TPart> handler) where TPart : class, IContent {
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
        }

        class InlineTemplateFilter<TPart> : TemplateFilterBase<TPart> where TPart : class, IContent {
            public Action<GetItemMetadataContext, TPart> OnGetItemMetadata { get; set; }
            public Action<BuildDisplayModelContext, TPart> OnGetDisplayViewModel { get; set; }
            public Action<BuildEditorModelContext, TPart> OnGetEditorViewModel { get; set; }
            public Action<UpdateEditorModelContext, TPart> OnUpdateEditorViewModel { get; set; }
            protected override void GetItemMetadata(GetItemMetadataContext context, TPart instance) {
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


        void IContentHandler.GetItemMetadata(GetItemMetadataContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.GetItemMetadata(context);
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

        protected virtual void Loading(LoadContentContext context) { }
        protected virtual void Loaded(LoadContentContext context) { }

        protected virtual void Creating(CreateContentContext context) { }
        protected virtual void Created(CreateContentContext context) { }

        protected virtual void GetItemMetadata(GetItemMetadataContext context) { }
        protected virtual void BuildDisplayModel(BuildDisplayModelContext context) { }
        protected virtual void BuildEditorModel(BuildEditorModelContext context) { }
        protected virtual void UpdateEditorModel(UpdateEditorModelContext context) {}
    }
}