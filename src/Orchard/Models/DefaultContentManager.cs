using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Orchard.Data;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Models.ViewModels;
using Orchard.UI.Navigation;

namespace Orchard.Models {
    public class DefaultContentManager : IContentManager {
        private readonly IContext _context;
        private readonly IRepository<ContentItemRecord> _contentItemRepository;
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;

        public DefaultContentManager(
            IContext context,
            IRepository<ContentItemRecord> contentItemRepository,
            IRepository<ContentTypeRecord> contentTypeRepository) {
            _context = context;
            _contentItemRepository = contentItemRepository;
            _contentTypeRepository = contentTypeRepository;
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
            // obtain root record to determine the model type
            var record = _contentItemRepository.Get(id);

            // no record of that id means content item doesn't exist
            if (record == null)
                return null;

            // allocate instance and set record property
            var contentItem = New(record.ContentType.Name);
            contentItem.Id = record.Id;
            contentItem.Record = record;

            // create a context with a new instance to load            
            var context = new LoadContentContext {
                Id = contentItem.Id,
                ContentType = contentItem.ContentType,
                ContentItemRecord = record,
                ContentItem = contentItem,
            };

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            foreach (var handler in Handlers) {
                handler.Loading(context);
            }
            foreach (var handler in Handlers) {
                handler.Loaded(context);
            }

            return context.ContentItem;
        }

        public void Create(ContentItem contentItem) {
            // produce root record to determine the model id
            var modelRecord = new ContentItemRecord { ContentType = AcquireContentTypeRecord(contentItem.ContentType) };
            _contentItemRepository.Create(modelRecord);
            contentItem.Record = modelRecord;

            // build a context with the initialized instance to create
            var context = new CreateContentContext {
                Id = modelRecord.Id,
                ContentType = modelRecord.ContentType.Name,
                ContentItemRecord = modelRecord,
                ContentItem = contentItem
            };

            // set the id
            context.ContentItem.Id = context.Id;


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

        public ItemDisplayModel<TContentPart> BuildDisplayModel<TContentPart>(TContentPart content, string groupName, string displayType) where TContentPart : IContent {
            return BuildDisplayModel(content, groupName, displayType, null);
        }

        public ItemDisplayModel<TContent> BuildDisplayModel<TContent>(TContent content, string groupName, string displayType, string templatePath) where TContent : IContent {
            var itemView = new ItemDisplayModel<TContent> { Item = content, Displays = Enumerable.Empty<TemplateViewModel>() };
            var context = new BuildDisplayModelContext(itemView, groupName, displayType, templatePath);
            foreach (var handler in Handlers) {
                handler.BuildDisplayModel(context);
            }
            context.DisplayModel.Displays = OrderTemplates(context.DisplayModel.Displays);
            return itemView;
        }

        public ItemEditorModel<TContent> BuildEditorModel<TContent>(TContent content, string groupName) where TContent : IContent {
            return BuildEditorModel(content, groupName, null);
        }

        public ItemEditorModel<TContent> BuildEditorModel<TContent>(TContent content, string groupName, string templatePath) where TContent : IContent {
            var itemView = new ItemEditorModel<TContent> { Item = content, Editors = Enumerable.Empty<TemplateViewModel>() };
            var context = new BuildEditorModelContext(itemView, groupName, templatePath);
            foreach (var handler in Handlers) {
                handler.BuildEditorModel(context);
            }
            context.EditorModel.Editors = OrderTemplates(context.EditorModel.Editors);
            return itemView;
        }

        public ItemEditorModel<TContent> UpdateEditorModel<TContent>(TContent content, string groupName, IUpdateModel updater) where TContent : IContent {
            return UpdateEditorModel(content, groupName, updater, null);
        }

        public ItemEditorModel<TContent> UpdateEditorModel<TContent>(TContent content, string groupName, IUpdateModel updater, string templatePath) where TContent : IContent {
            var itemView = new ItemEditorModel<TContent> { Item = content, Editors = Enumerable.Empty<TemplateViewModel>() };
            var context = new UpdateEditorModelContext(itemView, groupName, updater, templatePath);
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
