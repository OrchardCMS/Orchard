using Orchard.ContentManagement.Records;
using Orchard.Data;

namespace Orchard.ContentManagement.Handlers {
    public class StorageFilter<TRecord> : StorageFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
        private readonly IRepository<TRecord> _repository;

        public StorageFilter(IRepository<TRecord> repository) {
            _repository = repository;
        }

        public bool AutomaticallyCreateMissingRecord { get; set; }

        protected override void Activated(ActivatedContentContext context, ContentPart<TRecord> instance) {
            instance.Record = new TRecord();
        }

        protected override void Creating(CreateContentContext context, ContentPart<TRecord> instance) {
            instance.Record.ContentItemRecord = context.ContentItemRecord;
            _repository.Create(instance.Record);
        }

        protected override void Loading(LoadContentContext context, ContentPart<TRecord> instance) {
            var record = _repository.Get(context.Id);
            if (record != null) {
                instance.Record = record;
            }
            else if (AutomaticallyCreateMissingRecord) {
                instance.Record.ContentItemRecord = context.ContentItemRecord;
                _repository.Create(instance.Record);
            }
        }

        protected override void Versioning(VersionContentContext context, ContentPart<TRecord> existing, ContentPart<TRecord> building) {
            building.Record = existing.Record;
        }
    }

    public class StorageVersionFilter<TRecord> : StorageFilterBase<ContentPart<TRecord>> where TRecord : ContentPartVersionRecord, new() {
        private readonly IRepository<TRecord> _repository;

        public StorageVersionFilter(IRepository<TRecord> repository) {
            _repository = repository;
        }

        public bool AutomaticallyCreateMissingRecord { get; set; }

        protected override void Activated(ActivatedContentContext context, ContentPart<TRecord> instance) {
            instance.Record = new TRecord();
        }

        protected override void Creating(CreateContentContext context, ContentPart<TRecord> instance) {
            instance.Record.ContentItemRecord = context.ContentItemRecord;
            instance.Record.ContentItemVersionRecord = context.ContentItemVersionRecord;
            _repository.Create(instance.Record);
        }

        protected override void Loading(LoadContentContext context, ContentPart<TRecord> instance) {
            var record = _repository.Get(context.ContentItemVersionRecord.Id);
            if (record != null) {
                instance.Record = record;
            }
            else if (AutomaticallyCreateMissingRecord) {
                instance.Record.ContentItemRecord = context.ContentItemRecord;
                instance.Record.ContentItemVersionRecord = context.ContentItemVersionRecord;
                _repository.Create(instance.Record);
            }
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
