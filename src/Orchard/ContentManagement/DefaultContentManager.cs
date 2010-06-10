using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement {
    public class DefaultContentManager : IContentManager {
        private readonly IComponentContext _context;
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly IRepository<ContentItemRecord> _contentItemRepository;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Func<IContentManagerSession> _contentManagerSession;

        public DefaultContentManager(
            IComponentContext context,
            IRepository<ContentTypeRecord> contentTypeRepository,
            IRepository<ContentItemRecord> contentItemRepository,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository,
            IContentDefinitionManager contentDefinitionManager,
            Func<IContentManagerSession> contentManagerSession) {
            _context = context;
            _contentTypeRepository = contentTypeRepository;
            _contentItemRepository = contentItemRepository;
            _contentItemVersionRepository = contentItemVersionRepository;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManagerSession = contentManagerSession;
        }

        private IEnumerable<IContentHandler> _handlers;
        public IEnumerable<IContentHandler> Handlers {
            get {
                if (_handlers == null)
                    _handlers = _context.Resolve<IEnumerable<IContentHandler>>();
                return _handlers;
            }
        }

        public IEnumerable<ContentType> GetContentTypes() {
            return Handlers.Aggregate(
                Enumerable.Empty<ContentType>(),
                (types, handler) => types.Concat(handler.GetContentTypes()));
        }

        public virtual ContentItem New(string contentType) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null) {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            // create a new kernel for the model instance
            var context = new ActivatingContentContext {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
                Builder = new ContentItemBuilder(contentTypeDefinition)
            };

            // invoke handlers to weld aspects onto kernel
            foreach (var handler in Handlers) {
                handler.Activating(context);
            }
            var context2 = new ActivatedContentContext {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };

            // back-reference for convenience (e.g. getting metadata when in a view)
            context2.ContentItem.ContentManager = this;

            foreach (var handler in Handlers) {
                handler.Activated(context2);
            }

            // composite result is returned
            return context2.ContentItem;
        }

        public virtual ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public virtual ContentItem Get(int id, VersionOptions options) {
            var session = _contentManagerSession();
            ContentItem contentItem;

            ContentItemVersionRecord versionRecord = null;

            // obtain the root records based on version options
            if (options.VersionRecordId != 0) {
                // short-circuit if item held in session
                if (session.RecallVersionRecordId(options.VersionRecordId, out contentItem))
                    return contentItem;

                // locate explicit version record
                versionRecord = _contentItemVersionRepository.Get(options.VersionRecordId);
            }
            else {
                var record = _contentItemRepository.Get(id);
                if (record != null)
                    versionRecord = GetVersionRecord(options, record);
            }

            // no record means content item doesn't exist
            if (versionRecord == null) {
                return null;
            }

            // return item if obtained earlier in session
            if (session.RecallVersionRecordId(versionRecord.Id, out contentItem)) {
                return contentItem;
            }


            // allocate instance and set record property
            contentItem = New(versionRecord.ContentItemRecord.ContentType.Name);
            contentItem.VersionRecord = versionRecord;

            // store in session prior to loading to avoid some problems with simple circular dependencies
            session.Store(contentItem);

            // create a context with a new instance to load            
            var context = new LoadContentContext(contentItem);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            foreach (var handler in Handlers) {
                handler.Loading(context);
            }
            foreach (var handler in Handlers) {
                handler.Loaded(context);
            }

            // when draft is required and latest is published a new version is appended 
            if (options.IsDraftRequired && versionRecord.Published) {
                return BuildNewVersion(context.ContentItem);
            }

            return context.ContentItem;
        }

        private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord) {
            if (options.IsPublished) {
                return itemRecord.Versions.FirstOrDefault(
                           x => x.Published) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Published);
            }
            if (options.IsLatest || options.IsDraftRequired) {
                return itemRecord.Versions.FirstOrDefault(
                           x => x.Latest) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Latest);
            }
            if (options.IsDraft) {
                return itemRecord.Versions.FirstOrDefault(
                           x => x.Latest && !x.Published) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Latest && !x.Published);
            }
            if (options.VersionNumber != 0) {
                return itemRecord.Versions.FirstOrDefault(
                           x => x.Number == options.VersionNumber) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Number == options.VersionNumber);
            }
            return null;
        }

        public virtual IEnumerable<ContentItem> GetAllVersions(int id) {
            return _contentItemVersionRepository
                .Fetch(x => x.ContentItemRecord.Id == id)
                .OrderBy(x => x.Number)
                .Select(x => Get(x.ContentItemRecord.Id, VersionOptions.VersionRecord(x.Id)));
        }

        public virtual void Publish(ContentItem contentItem) {
            if (contentItem.VersionRecord.Published) {
                return;
            }
            // create a context for the item and it's previous published record
            var previous = contentItem.Record.Versions.SingleOrDefault(x => x.Published);
            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            foreach (var handler in Handlers) {
                handler.Publishing(context);
            }

            if (previous != null) {
                previous.Published = false;
            }
            contentItem.VersionRecord.Published = true;

            foreach (var handler in Handlers) {
                handler.Published(context);
            }
        }

        public virtual void Unpublish(ContentItem contentItem) {
            ContentItem publishedItem;
            if (contentItem.VersionRecord.Published) {
                // the version passed in is the published one
                publishedItem = contentItem;
            }
            else {
                // try to locate the published version of this item
                publishedItem = Get(contentItem.Id, VersionOptions.Published);
            }

            if (publishedItem == null) {
                // no published version exists. no work to perform.
                return;
            }

            // create a context for the item. the publishing version is null in this case
            // and the previous version is the one active prior to unpublishing. handlers
            // should take this null check into account
            var context = new PublishContentContext(contentItem, publishedItem.VersionRecord) {
                PublishingItemVersionRecord = null
            };

            foreach (var handler in Handlers) {
                handler.Publishing(context);
            }

            publishedItem.VersionRecord.Published = false;

            foreach (var handler in Handlers) {
                handler.Published(context);
            }
        }

        public virtual void Remove(ContentItem contentItem) {
            var activeVersions = _contentItemVersionRepository.Fetch(x => x.ContentItemRecord == contentItem.Record && (x.Published || x.Latest));
            var context = new RemoveContentContext(contentItem);

            foreach (var handler in Handlers) {
                handler.Removing(context);
            }

            foreach (var version in activeVersions) {
                if (version.Published) {
                    version.Published = false;
                }
                if (version.Latest) {
                    version.Latest = false;
                }
            }

            foreach (var handler in Handlers) {
                handler.Removed(context);
            }
        }

        protected virtual ContentItem BuildNewVersion(ContentItem existingContentItem) {
            var contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            var existingItemVersionRecord = existingContentItem.VersionRecord;
            var buildingItemVersionRecord = new ContentItemVersionRecord {
                ContentItemRecord = contentItemRecord,
                Latest = true,
                Published = false
            };


            var latestVersion = contentItemRecord.Versions.SingleOrDefault(x => x.Latest);

            if (latestVersion != null) {
                latestVersion.Latest = false;
                buildingItemVersionRecord.Number = latestVersion.Number + 1;
            }
            else {
                buildingItemVersionRecord.Number = contentItemRecord.Versions.Max(x => x.Number) + 1;
            }

            contentItemRecord.Versions.Add(buildingItemVersionRecord);
            _contentItemVersionRepository.Create(buildingItemVersionRecord);

            var buildingContentItem = New(existingContentItem.ContentType);
            buildingContentItem.VersionRecord = buildingItemVersionRecord;

            var context = new VersionContentContext {
                Id = existingContentItem.Id,
                ContentType = existingContentItem.ContentType,
                ContentItemRecord = contentItemRecord,
                ExistingContentItem = existingContentItem,
                BuildingContentItem = buildingContentItem,
                ExistingItemVersionRecord = existingItemVersionRecord,
                BuildingItemVersionRecord = buildingItemVersionRecord,
            };
            foreach (var handler in Handlers) {
                handler.Versioning(context);
            }
            foreach (var handler in Handlers) {
                handler.Versioned(context);
            }

            return context.BuildingContentItem;
        }

        public virtual void Create(ContentItem contentItem) {
            Create(contentItem, VersionOptions.Published);
        }

        public virtual void Create(ContentItem contentItem, VersionOptions options) {
            // produce root record to determine the model id
            contentItem.VersionRecord = new ContentItemVersionRecord {
                ContentItemRecord = new ContentItemRecord {
                    ContentType = AcquireContentTypeRecord(contentItem.ContentType)
                },
                Number = 1,
                Latest = true,
                Published = true
            };
            // add to the collection manually for the created case
            contentItem.VersionRecord.ContentItemRecord.Versions.Add(contentItem.VersionRecord);

            // version may be specified
            if (options.VersionNumber != 0) {
                contentItem.VersionRecord.Number = options.VersionNumber;
            }

            // draft flag on create is required for explicitly-published content items
            if (options.IsDraft) {
                contentItem.VersionRecord.Published = false;
            }

            _contentItemRepository.Create(contentItem.Record);
            _contentItemVersionRepository.Create(contentItem.VersionRecord);


            // build a context with the initialized instance to create
            var context = new CreateContentContext(contentItem);


            // invoke handlers to add information to persistent stores
            foreach (var handler in Handlers) {
                handler.Creating(context);
            }
            foreach (var handler in Handlers) {
                handler.Created(context);
            }

            if (options.IsPublished) {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                foreach (var handler in Handlers) {
                    handler.Publishing(publishContext);
                }

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                foreach (var handler in Handlers) {
                    handler.Published(publishContext);
                }
            }
        }

        public ContentItemMetadata GetItemMetadata(IContent content) {
            var context = new GetContentItemMetadataContext {
                ContentItem = content.ContentItem,
                Metadata = new ContentItemMetadata()
            };
            foreach (var handler in Handlers) {
                handler.GetContentItemMetadata(context);
            }
            return context.Metadata;
        }

        public ContentItemViewModel<TContentPart> BuildDisplayModel<TContentPart>(TContentPart content, string displayType) where TContentPart : IContent {
            var displayModel = new ContentItemViewModel<TContentPart>(content);
            var context = new BuildDisplayModelContext(displayModel, displayType);
            foreach (var handler in Handlers) {
                handler.BuildDisplayModel(context);
            }
            return displayModel;
        }

        public ContentItemViewModel<TContentPart> BuildEditorModel<TContentPart>(TContentPart content) where TContentPart : IContent {
            var editorModel = new ContentItemViewModel<TContentPart>(content);
            var context = new BuildEditorModelContext(editorModel);
            foreach (var handler in Handlers) {
                handler.BuildEditorModel(context);
            }
            return editorModel;
        }

        public ContentItemViewModel<TContentPart> UpdateEditorModel<TContentPart>(TContentPart content, IUpdateModel updater) where TContentPart : IContent {
            var editorModel = new ContentItemViewModel<TContentPart>(content);

            var context = new UpdateEditorModelContext(editorModel, updater);
            foreach (var handler in Handlers) {
                handler.UpdateEditorModel(context);
            }
            return editorModel;
        }

        public IContentQuery<ContentItem> Query() {
            var query = _context.Resolve<IContentQuery>(TypedParameter.From<IContentManager>(this));
            return query.ForPart<ContentItem>();
        }

        public void Flush() {
            _contentItemRepository.Flush();
        }

        private ContentTypeRecord AcquireContentTypeRecord(string contentType) {
            var contentTypeRecord = _contentTypeRepository.Get(x => x.Name == contentType);
            if (contentTypeRecord == null) {
                //TEMP: this is not safe... ContentItem types could be created concurrently?
                contentTypeRecord = new ContentTypeRecord { Name = contentType };
                _contentTypeRepository.Create(contentTypeRecord);
            }
            return contentTypeRecord;
        }
    }
}
