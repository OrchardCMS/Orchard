using Orchard.Data;
using Orchard.Models.Records;

namespace Orchard.Models.Driver {
    public abstract class ModelDriverWithRecord<TRecord> : ModelDriver where TRecord : ModelPartRecord, new() {
        private readonly IRepository<TRecord> _repository;

        protected ModelDriverWithRecord(IRepository<TRecord> repository) {
            _repository = repository;
        }

        protected override void Newed(NewedModelContext context) {
            var instance = context.Instance.As<ModelPartWithRecord<TRecord>>();
            if (instance != null && instance.Record == null) {
                instance.Record = new TRecord();
            }
        }

        protected override void Create(CreateModelContext context) {
            var instance = context.Instance.As<ModelPartWithRecord<TRecord>>();
            if (instance != null && instance.Record != null) {
                instance.Record.Model = context.Record;
                _repository.Create(instance.Record);
            }
        }

        protected override void Load(LoadModelContext context) {
            var instance = context.Instance.As<ModelPartWithRecord<TRecord>>();
            if (instance != null) {
                instance.Record = _repository.Get(context.Id);
            }
        }

    }
}