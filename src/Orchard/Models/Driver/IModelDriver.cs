namespace Orchard.Models.Driver {
    public interface IModelDriver : IDependency {
        void Activating(ActivatingModelContext context);
        void Activated(ActivatedModelContext context);
        void Creating(CreateModelContext context);
        void Created(CreateModelContext context);
        void Loading(LoadModelContext context);
        void Loaded(LoadModelContext context);

        void GetEditors(GetModelEditorsContext context);
        void UpdateEditors(UpdateModelContext context);
    }
}