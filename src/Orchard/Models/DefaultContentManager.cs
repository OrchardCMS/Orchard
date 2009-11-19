using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using Orchard.Data;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.UI.Models;

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

        private IEnumerable<IContentHandler> _drivers;
        public IEnumerable<IContentHandler> Drivers {
            get {
                if (_drivers == null)
                    _drivers = _context.Resolve<IEnumerable<IContentHandler>>();
                return _drivers;
            }            
        }

        public virtual ContentItem New(string contentType) {

            // create a new kernel for the model instance
            var context = new ActivatingContentContext {
                ContentType = contentType,
                Builder = new ContentItemBuilder(contentType)
            };

            // invoke drivers to weld aspects onto kernel
            foreach (var driver in Drivers) {
                driver.Activating(context);
            }
            var context2 = new ActivatedContentContext {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };
            foreach (var driver in Drivers) {
                driver.Activated(context2);
            }

            // composite result is returned
            return context2.ContentItem;
        }

        public virtual ContentItem Get(int id) {
            // obtain root record to determine the model type
            var contentItemRecord = _contentItemRepository.Get(id);

            // create a context with a new instance to load
            var context = new LoadContentContext {
                Id = contentItemRecord.Id,
                ContentType = contentItemRecord.ContentType.Name,
                ContentItemRecord = contentItemRecord,
                ContentItem = New(contentItemRecord.ContentType.Name)
            };

            // set the id
            context.ContentItem.Id = context.Id;

            // invoke drivers to acquire state, or at least establish lazy loading callbacks
            foreach (var driver in Drivers) {
                driver.Loading(context);
            }
            foreach (var driver in Drivers) {
                driver.Loaded(context);
            }

            return context.ContentItem;
        }

        public void Create(ContentItem contentItem) {
            // produce root record to determine the model id
            var modelRecord = new ContentItemRecord { ContentType = AcquireContentTypeRecord(contentItem.ContentType) };
            _contentItemRepository.Create(modelRecord);

            // build a context with the initialized instance to create
            var context = new CreateContentContext {
                Id = modelRecord.Id,
                ContentType = modelRecord.ContentType.Name,
                ContentItemRecord = modelRecord,
                ContentItem = contentItem
            };

            // set the id
            context.ContentItem.Id = context.Id;


            // invoke drivers to add information to persistent stores
            foreach (var driver in Drivers) {
                driver.Creating(context);
            }
            foreach (var driver in Drivers) {
                driver.Created(context);
            }
        }


        public IEnumerable<ModelTemplate> GetDisplays(ContentItem contentItem) {
            var context = new GetDisplaysContext(contentItem);
            foreach (var driver in Drivers) {
                driver.GetDisplays(context);
            }
            return context.Displays;
        }

        public IEnumerable<ModelTemplate> GetEditors(ContentItem contentItem) {
            var context = new GetEditorsContext(contentItem);
            foreach (var driver in Drivers) {
                driver.GetEditors(context);
            }
            return context.Editors;
        }

        public IEnumerable<ModelTemplate> UpdateEditors(ContentItem contentItem, IUpdateModel updater) {
            var context = new UpdateContentContext(contentItem, updater);
            foreach (var driver in Drivers) {
                driver.UpdateEditors(context);
            }
            return context.Editors;
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
