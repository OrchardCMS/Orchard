using Orchard.ContentManagement.Records;
using Orchard.Data;

namespace Orchard.ContentManagement.Handlers {
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