using System;
using Orchard.ContentManagement.Records;
using Orchard.Data;

namespace Orchard.ContentManagement.Handlers {
    public static class StorageFilter {
        public static StorageFilter<TRecord> For<TRecord>(IRepository<TRecord> repository) where TRecord : ContentPartRecord, new() {
            if (typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
                var filterType = typeof(StorageVersionFilter<>).MakeGenericType(typeof(TRecord));
                return (StorageFilter<TRecord>)Activator.CreateInstance(filterType, repository);
            }
            return new StorageFilter<TRecord>(repository);
        }
    }

    public class StorageFilter<TRecord> : StorageFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
        protected readonly IRepository<TRecord> _repository;

        public StorageFilter(IRepository<TRecord> repository) {
            _repository = repository;
        }


        protected override void Activated(ActivatedContentContext context, ContentPart<TRecord> instance) {
            instance.Record = new TRecord();
        }

        protected override void Creating(CreateContentContext context, ContentPart<TRecord> instance) {
            instance.Record.ContentItemRecord = context.ContentItemRecord;
            _repository.Create(instance.Record);
        }

        protected virtual TRecord GetRecord(LoadContentContext context) {
            return _repository.Get(context.Id);
        }

        protected override void Loading(LoadContentContext context, ContentPart<TRecord> instance) {
            var record = GetRecord(context);
            if (record != null) {
                instance.Record = record;
            }
            else {
                var createContext = new CreateContentContext {
                    ContentItem = context.ContentItem,
                    ContentItemRecord = context.ContentItemRecord,
                    ContentItemVersionRecord = context.ContentItemVersionRecord,
                    ContentType = context.ContentType,
                    Id = context.Id
                };
                Creating(createContext, instance);
            }
        }

        protected override void Versioning(VersionContentContext context, ContentPart<TRecord> existing, ContentPart<TRecord> building) {
            building.Record = existing.Record;
        }
    }

    public class StorageVersionFilter<TRecord> : StorageFilter<TRecord> where TRecord : ContentPartVersionRecord, new() {
        public StorageVersionFilter(IRepository<TRecord> repository)
            : base(repository) {
        }

        protected override TRecord GetRecord(LoadContentContext context) {
            return _repository.Get(context.ContentItemVersionRecord.Id);
        }

        protected override void Creating(CreateContentContext context, ContentPart<TRecord> instance) {
            instance.Record.ContentItemRecord = context.ContentItemRecord;
            instance.Record.ContentItemVersionRecord = context.ContentItemVersionRecord;
            _repository.Create(instance.Record);
        }

        protected override void Versioning(VersionContentContext context, ContentPart<TRecord> existing, ContentPart<TRecord> building) {
            // move known ORM values over
            _repository.Copy(existing.Record, building.Record);

            // only the up-reference to the particular version differs at this point
            building.Record.ContentItemVersionRecord = context.BuildingItemVersionRecord;

            // push the new instance into the transaction and session
            _repository.Create(building.Record);
        }
    }
}
