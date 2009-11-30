using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;

namespace Orchard.Models.Driver {
    public abstract class ContentProvider : IContentProvider {
        protected ContentProvider() {
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

        protected void OnGetDisplays<TPart>(Action<GetDisplaysContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnGetDisplays = handler });
        }

        protected void OnGetEditors<TPart>(Action<GetEditorsContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnGetEditors = handler });
        }

        protected void OnUpdateEditors<TPart>(Action<UpdateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineTemplateFilter<TPart> { OnUpdateEditors = handler });
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
                if (OnCreating != null) OnCreated(context, instance);
            }
            protected override void Loading(LoadContentContext context, TPart instance) {
                if (OnLoaded != null) OnLoading(context, instance);
            }
            protected override void Loaded(LoadContentContext context, TPart instance) {
                if (OnLoaded != null) OnLoaded(context, instance);
            }
        }

        class InlineTemplateFilter<TPart> : TemplateFilterBase<TPart> where TPart : class, IContent {
            public Action<GetDisplaysContext, TPart> OnGetDisplays { get; set; }
            public Action<GetEditorsContext, TPart> OnGetEditors { get; set; }
            public Action<UpdateContentContext, TPart> OnUpdateEditors { get; set; }
            protected override void GetDisplays(GetDisplaysContext context, TPart instance) {
                if (OnGetDisplays != null) OnGetDisplays(context, instance);
            }
            protected override void GetEditors(GetEditorsContext context, TPart instance) {
                if (OnGetEditors != null) OnGetEditors(context, instance);
            }
            protected override void UpdateEditors(UpdateContentContext context, TPart instance) {
                if (OnUpdateEditors != null) OnUpdateEditors(context, instance);
            }
        }

        public virtual IEnumerable<ContentType> GetContentTypes() {
            return Enumerable.Empty<ContentType>();
        }

        void IContentProvider.Activating(ActivatingContentContext context) {
            foreach (var filter in Filters.OfType<IContentActivatingFilter>())
                filter.Activating(context);
            Activating(context);
        }

        void IContentProvider.Activated(ActivatedContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Activated(context);
            Activated(context);
        }

        void IContentProvider.Creating(CreateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Creating(context);
            Creating(context);
        }

        void IContentProvider.Created(CreateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Created(context);
            Created(context);
        }

        void IContentProvider.Loading(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loading(context);
            Loading(context);
        }

        void IContentProvider.Loaded(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loaded(context);
            Loaded(context);
        }


        void IContentProvider.GetDisplays(GetDisplaysContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.GetDisplays(context);
            GetDisplays(context);
        }
        void IContentProvider.GetEditors(GetEditorsContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.GetEditors(context);
            GetEditors(context);
        }
        void IContentProvider.UpdateEditors(UpdateContentContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.UpdateEditors(context);
            UpdateEditors(context);
        }

        protected virtual void Activating(ActivatingContentContext context) { }
        protected virtual void Activated(ActivatedContentContext context) { }

        protected virtual void Loading(LoadContentContext context) { }
        protected virtual void Loaded(LoadContentContext context) { }

        protected virtual void Creating(CreateContentContext context) { }
        protected virtual void Created(CreateContentContext context) { }

        protected virtual void GetDisplays(GetDisplaysContext context) { }
        protected virtual void GetEditors(GetEditorsContext context) { }
        protected virtual void UpdateEditors(UpdateContentContext context) {}
    }
}