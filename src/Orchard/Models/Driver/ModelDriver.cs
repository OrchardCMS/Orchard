using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;

namespace Orchard.Models.Driver {
    public abstract class ModelDriver : IModelDriver {
        protected ModelDriver() {
            Filters = new List<IModelFilter>();
            Logger = NullLogger.Instance;
        }

        public List<IModelFilter> Filters { get; set; }
        public ILogger Logger { get; set; }

        public void AddOnActivated<TPart>(Action<ActivatedModelContext, TPart> handler) where TPart : class, IContentItemPart {
            Filters.Add(new InlineStorageFilter<TPart> { OnActivated = handler });
        }
        public void AddOnCreating<TPart>(Action<CreateModelContext, TPart> handler) where TPart : class, IContentItemPart {
            Filters.Add(new InlineStorageFilter<TPart> { OnCreating = handler });
        }
        public void AddOnLoaded<TPart>(Action<LoadModelContext, TPart> handler) where TPart : class, IContentItemPart {
            Filters.Add(new InlineStorageFilter<TPart> { OnLoaded = handler });
        }

        class InlineStorageFilter<TPart> : StorageFilterBase<TPart> where TPart : class, IContentItemPart {
            public Action<ActivatedModelContext, TPart> OnActivated { get; set; }
            public Action<CreateModelContext, TPart> OnCreating { get; set; }
            public Action<CreateModelContext, TPart> OnCreated { get; set; }
            public Action<LoadModelContext, TPart> OnLoading { get; set; }
            public Action<LoadModelContext, TPart> OnLoaded { get; set; }
            protected override void Activated(ActivatedModelContext context, TPart instance) { if (OnActivated != null) OnActivated(context, instance); }
            protected override void Creating(CreateModelContext context, TPart instance) { if (OnCreating != null) OnCreating(context, instance); }
            protected override void Loaded(LoadModelContext context, TPart instance) { if (OnLoaded != null) OnLoaded(context, instance); }
        }

        void IModelDriver.Activating(ActivatingModelContext context) {
            foreach (var filter in Filters.OfType<IModelActivatingFilter>())
                filter.Activating(context);
            Activating(context);
        }

        void IModelDriver.Activated(ActivatedModelContext context) {
            foreach (var filter in Filters.OfType<IModelStorageFilter>())
                filter.Activated(context);
            Activated(context);
        }

        void IModelDriver.Creating(CreateModelContext context) {
            foreach (var filter in Filters.OfType<IModelStorageFilter>())
                filter.Creating(context);
            Creating(context);
        }

        void IModelDriver.Created(CreateModelContext context) {
            foreach (var filter in Filters.OfType<IModelStorageFilter>())
                filter.Created(context);
            Created(context);
        }

        void IModelDriver.Loading(LoadModelContext context) {
            foreach (var filter in Filters.OfType<IModelStorageFilter>())
                filter.Loading(context);
            Loading(context);
        }

        void IModelDriver.Loaded(LoadModelContext context) {
            foreach (var filter in Filters.OfType<IModelStorageFilter>())
                filter.Loaded(context);
            Loaded(context);
        }

        void IModelDriver.GetEditors(GetModelEditorsContext context) { GetEditors(context); }
        void IModelDriver.UpdateEditors(UpdateModelContext context) { UpdateEditors(context); }

        protected virtual void Activating(ActivatingModelContext context) { }
        protected virtual void Activated(ActivatedModelContext context) { }

        protected virtual void Loading(LoadModelContext context) { }
        protected virtual void Loaded(LoadModelContext context) { }

        protected virtual void Creating(CreateModelContext context) { }
        protected virtual void Created(CreateModelContext context) { }

        protected virtual void GetEditors(GetModelEditorsContext context) {}

        protected virtual void UpdateEditors(UpdateModelContext context) {}
    }
}