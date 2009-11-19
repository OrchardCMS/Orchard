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

        public void AddOnActivated<TPart>(Action<ActivatedContentContext, TPart> handler) where TPart : class, IContentItemPart {
            Filters.Add(new InlineStorageFilter<TPart> { OnActivated = handler });
        }
        public void AddOnCreating<TPart>(Action<CreateContentContext, TPart> handler) where TPart : class, IContentItemPart {
            Filters.Add(new InlineStorageFilter<TPart> { OnCreating = handler });
        }
        public void AddOnLoaded<TPart>(Action<LoadContentContext, TPart> handler) where TPart : class, IContentItemPart {
            Filters.Add(new InlineStorageFilter<TPart> { OnLoaded = handler });
        }

        class InlineStorageFilter<TPart> : StorageFilterBase<TPart> where TPart : class, IContentItemPart {
            public Action<ActivatedContentContext, TPart> OnActivated { get; set; }
            public Action<CreateContentContext, TPart> OnCreating { get; set; }
            public Action<CreateContentContext, TPart> OnCreated { get; set; }
            public Action<LoadContentContext, TPart> OnLoading { get; set; }
            public Action<LoadContentContext, TPart> OnLoaded { get; set; }
            protected override void Activated(ActivatedContentContext context, TPart instance) { if (OnActivated != null) OnActivated(context, instance); }
            protected override void Creating(CreateContentContext context, TPart instance) { if (OnCreating != null) OnCreating(context, instance); }
            protected override void Loaded(LoadContentContext context, TPart instance) { if (OnLoaded != null) OnLoaded(context, instance); }
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

        void IContentHandler.GetEditors(GetContentEditorsContext context) {
            foreach (var filter in Filters.OfType<IContentTemplateFilter>())
                filter.GetEditors(context);
            GetEditors(context);
        }
        void IContentHandler.UpdateEditors(UpdateContentContext context) {
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

        protected virtual void GetEditors(GetContentEditorsContext context) {}

        protected virtual void UpdateEditors(UpdateContentContext context) {}
    }
}