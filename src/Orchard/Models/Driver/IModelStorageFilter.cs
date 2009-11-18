namespace Orchard.Models.Driver {
    public interface IModelStorageFilter : IModelFilter {
        void Activated(ActivatedModelContext context);
        void Creating(CreateModelContext context);
        void Created(CreateModelContext context);
        void Loading(LoadModelContext context);
        void Loaded(LoadModelContext context);        
    }
}
