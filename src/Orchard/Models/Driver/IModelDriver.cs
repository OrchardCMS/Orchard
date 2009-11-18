namespace Orchard.Models.Driver {
    public interface IModelDriver : IDependency {
        void New(NewModelContext context);
        void Newed(NewedModelContext context);
        void Create(CreateModelContext context);
        void Created(CreateModelContext context);
        void Load(LoadModelContext context);
        void Loaded(LoadModelContext context);

        void GetEditors(GetModelEditorsContext context);
        void UpdateEditors(UpdateModelContext context);
    }
}