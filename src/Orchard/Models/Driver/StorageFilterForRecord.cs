using Orchard.Data;
using Orchard.Models.Records;

namespace Orchard.Models.Driver {
    public class StorageFilterForRecord<TRecord> : StorageFilterBase<ModelPartWithRecord<TRecord>> where TRecord : ModelPartRecord,new() {
        private readonly IRepository<TRecord> _repository;

        public StorageFilterForRecord(IRepository<TRecord> repository) {
            _repository = repository;
        }

        public bool AutomaticallyCreateMissingRecord { get; set; }

        protected override void Activated(ActivatedModelContext context, ModelPartWithRecord<TRecord> instance) {
            instance.Record = new TRecord();
        }

        protected override void Creating(CreateModelContext context, ModelPartWithRecord<TRecord> instance) {
            instance.Record.Model = context.ModelRecord;
            _repository.Create(instance.Record);
        }

        protected override void Loading(LoadModelContext context, ModelPartWithRecord<TRecord> instance) {
            instance.Record = _repository.Get(instance.Id);
            if (instance.Record == null && AutomaticallyCreateMissingRecord) {
                instance.Record = new TRecord {Model = context.ModelRecord};
                _repository.Create(instance.Record);
            }
        }
    }
}
