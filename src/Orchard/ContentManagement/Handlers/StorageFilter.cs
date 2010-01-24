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
            if (this.GetType() == typeof(StorageFilter<TRecord>) && typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
                throw new ArgumentException(
                    string.Format("Use {0} (or {1}.For<TRecord>()) for versionable record types", typeof (StorageVersionFilter<>).Name, typeof(StorageFilter).Name),
                    "repository");
            }

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
                var createContext = new CreateContentContext(context.ContentItem);
                Creating(createContext, instance);
            }
        }

        protected override void Versioning(VersionContentContext context, ContentPart<TRecord> existing, ContentPart<TRecord> building) {
            building.Record = existing.Record;
        }
    }
}
