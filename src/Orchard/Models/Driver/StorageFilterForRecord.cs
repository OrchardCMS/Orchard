using Orchard.Data;
using Orchard.Models.Records;

namespace Orchard.Models.Driver {
    public class StorageFilterForRecord<TRecord> : StorageFilterBase<ModelPartWithRecord<TRecord>> where TRecord : ModelPartRecord,new() {
        private readonly IRepository<TRecord> _repository;

        public StorageFilterForRecord(IRepository<TRecord> repository) {
            _repository = repository;
        }

        protected override void Activated(ActivatedModelContext context, ModelPartWithRecord<TRecord> instance) {
            instance.Record = new TRecord();
        }

        protected override void Creating(CreateModelContext context, ModelPartWithRecord<TRecord> instance) {
            instance.Record.Model = context.Record;
            _repository.Create(instance.Record);
        }

        protected override void Loading(LoadModelContext context, ModelPartWithRecord<TRecord> instance) {
            instance.Record = _repository.Get(instance.Id);
        }
    }
}
