using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.ViewModels;
using Orchard.Data;
using Orchard.UI.Navigation;

namespace Orchard.ContentManagement {
    public class DefaultContentManager : IContentManager {
        private readonly IContext _context;
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly IRepository<ContentItemRecord> _contentItemRepository;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;

        public DefaultContentManager(
            IContext context,
            IRepository<ContentTypeRecord> contentTypeRepository,
            IRepository<ContentItemRecord> contentItemRepository,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository) {
            _context = context;
            _contentTypeRepository = contentTypeRepository;
            _contentItemRepository = contentItemRepository;
            _contentItemVersionRepository = contentItemVersionRepository;
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

            // create a new kernel for the model instance
            var context = new ActivatingContentContext {
                ContentType = contentType,
                Builder = new ContentItemBuilder(contentType)
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
            ContentItemVersionRecord versionRecord = null;
            var appendLatestVersion = false;

            // obtain the root records based on version options
            if (options.VersionRecordId != 0) {
                // explicit version record known
                versionRecord = _contentItemVersionRepository.Get(options.VersionRecordId);
            }
            else {
                var record = _contentItemRepository.Get(id);
                if (options.IsPublished) {
                    versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord == record && x.Published);
                }
                else if (options.IsLatest) {
                    versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord == record && x.Latest);
                }
                else if (options.IsDraft || options.IsDraftRequired) {
                    versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord == record && x.Latest && !x.Published);
                    if (versionRecord == null && options.IsDraftRequired) {
                        versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord == record && x.Latest);
                        appendLatestVersion = true;
                    }
                }
                else if (options.VersionNumber != 0) {
                    versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord == record && x.Number == options.VersionNumber);
                }

                //TEMP: this is to transition people with old databases
                if (versionRecord == null && !record.Versions.Any() && options.IsPublished) {
                    versionRecord = new ContentItemVersionRecord {
                        ContentItemRecord = record,
                        Latest = true,
                        Published = true,
                        Number = 1
                    };
                    record.Versions.Add(versionRecord);
                    _contentItemVersionRepository.Create(versionRecord);
                }
            }

            // no record means content item doesn't exist
            if (versionRecord == null) {
                return null;
            }

            // allocate instance and set record property
            var contentItem = New(versionRecord.ContentItemRecord.ContentType.Name);
            contentItem.VersionRecord = versionRecord;

            // create a context with a new instance to load            
            var context = new LoadContentContext {
                Id = contentItem.Id,
                ContentType = contentItem.ContentType,
                ContentItem = contentItem,
                ContentItemRecord = contentItem.Record,
                ContentItemVersionRecord = contentItem.VersionRecord,
            };

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            foreach (var handler in Handlers) {
                handler.Loading(context);
            }
            foreach (var handler in Handlers) {
                handler.Loaded(context);
            }

            // when draft is required and not currently available a new version is appended 
            if (appendLatestVersion) {
                return AppendLatestVersion(context.ContentItem);
            }

            return context.ContentItem;
        }

        public virtual ContentItem AppendLatestVersion(ContentItem existingContentItem) {
            var contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            var existingItemVersionRecord = existingContentItem.VersionRecord;
            var buildingItemVersionRecord = new ContentItemVersionRecord {
                ContentItemRecord = contentItemRecord,
                Latest = true,
                Published = false
            };

            if (existingItemVersionRecord.Latest == false) {
                var latestVersion = _contentItemVersionRepository.Get(x => x.ContentItemRecord == contentItemRecord && x.Latest);
                latestVersion.Latest = false;
                buildingItemVersionRecord.Number = latestVersion.Number + 1;
            }
            else {
                existingItemVersionRecord.Latest = false;
                buildingItemVersionRecord.Number = existingItemVersionRecord.Number + 1;
            }


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
            var context = new CreateContentContext {
                Id = contentItem.Id,
                ContentType = contentItem.ContentType,
                ContentItemRecord = contentItem.Record,
                ContentItemVersionRecord = contentItem.VersionRecord,
                ContentItem = contentItem
            };


            // invoke handlers to add information to persistent stores
            foreach (var handler in Handlers) {
                handler.Creating(context);
            }
            foreach (var handler in Handlers) {
                handler.Created(context);
            }
        }

        public ContentItemMetadata GetItemMetadata(IContent content) {
            var context = new GetItemMetadataContext {
                ContentItem = content.ContentItem,
                Metadata = new ContentItemMetadata()
            };
            foreach (var handler in Handlers) {
                handler.GetItemMetadata(context);
            }
            return context.Metadata;
        }

        public ItemDisplayModel<TContentPart> BuildDisplayModel<TContentPart>(TContentPart content, string displayType) where TContentPart : IContent {
            var itemView = new ItemDisplayModel<TContentPart> { Item = content };
            var context = new BuildDisplayModelContext(itemView, displayType);
            foreach (var handler in Handlers) {
                handler.BuildDisplayModel(context);
            }
            return itemView;
        }

        public ItemEditorModel<TContentPart> BuildEditorModel<TContentPart>(TContentPart content) where TContentPart : IContent {
            var itemView = new ItemEditorModel<TContentPart> { Item = content, Editors = Enumerable.Empty<TemplateViewModel>() };
            var context = new BuildEditorModelContext(itemView);
            foreach (var handler in Handlers) {
                handler.BuildEditorModel(context);
            }
            context.EditorModel.Editors = OrderTemplates(context.EditorModel.Editors);
            return itemView;
        }

        public ItemEditorModel<TContentPart> UpdateEditorModel<TContentPart>(TContentPart content, IUpdateModel updater) where TContentPart : IContent {
            var itemView = new ItemEditorModel<TContentPart> { Item = content, Editors = Enumerable.Empty<TemplateViewModel>() };

            var context = new UpdateEditorModelContext(itemView, updater);
            foreach (var handler in Handlers) {
                handler.UpdateEditorModel(context);
            }
            context.EditorModel.Editors = OrderTemplates(context.EditorModel.Editors);
            return itemView;
        }

        private static IEnumerable<TemplateViewModel> OrderTemplates(IEnumerable<TemplateViewModel> templates) {
            var comparer = new PositionComparer();
            //TODO: rethink this comparison because it adds a requirement on naming zones.
            return templates.OrderBy(x => (x.ZoneName ?? "*") + "." + (x.Position ?? "5"), comparer);
        }

        public IContentQuery<ContentItem> Query() {
            var query = _context.Resolve<IContentQuery>(TypedParameter.From<IContentManager>(this));
            return query.ForPart<ContentItem>();
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
